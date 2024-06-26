using System.Buffers.Binary;
using AMQPClient.Protocol.Methods;
using Microsoft.Extensions.Logging;

namespace AMQPClient.Protocol;

public class AmqpFrameStream : IAmqpFrameSender, IDisposable, IAsyncDisposable
{
    private const byte FrameHeaderSize = 7;
    private readonly SemaphoreSlim _sendLock = new(1, 1);
    private readonly Stream _sourceStream;
    private byte[] _frameBody;
    public event Action FrameSent;
    public event Action FrameReceived;
    private ILogger<InternalConnection> _logger = DefaultLoggerFactory.CreateLogger<InternalConnection>();

    private readonly Dictionary<short, Queue<AmqpMethodFrame>> _partialFrames = new();

    public AmqpFrameStream(Stream sourceStream)
    {
        _sourceStream = sourceStream;
    }

    public Task SendFrameAsync(AmqpFrame frame)
    {
        return SendRawAsync(frame.ToBytes());
    }

    public async Task SendRawAsync(byte[] bytes)
    {
        await _sendLock.WaitAsync();

        try
        {
            await _sourceStream.WriteAsync(bytes);
            FrameSent?.Invoke();
        }
        finally
        {
            _sendLock.Release();
        }
    }

    public async Task<AmqpFrame?> ReadFrameAsync(CancellationToken cancellationToken = default)
    {
        var (type, channel, size) = await ReadFrameHeader(cancellationToken);
        _frameBody = (await ReadAsync(size, cancellationToken)).ToArray();
        // Read EOF frame
        await ReadAsync(1, cancellationToken);
        FrameReceived?.Invoke();

        var frameType = (FrameType)type;

        switch (frameType)
        {
            case FrameType.Method:
                return HandleMethodFrame(channel);
            case FrameType.ContentHeader:
                return HandleContentHeaderFrame(channel);
            case FrameType.Body:
                return HandleBodyFrame(channel);
            case FrameType.Heartbeat:
                _logger.LogInformation("***Heartbeat frame received...***");
                return null;
            default:
                throw new Exception("Unreachable");
        }
    }

    private AmqpMethodFrame? HandleMethodFrame(short channel)
    {
        var classId = BinaryPrimitives.ReadInt16BigEndian(_frameBody.AsSpan()[..2]);
        var methodId = BinaryPrimitives.ReadInt16BigEndian(_frameBody.AsSpan()[2..4]);
        var methodInfo = typeof(Decoder).GetMethod("CreateMethodFrame")!;
        var genericMethod = methodInfo.MakeGenericMethod(MethodMetaRegistry.GetMethodType(classId, methodId));
        var decodedMethod = (Method)genericMethod.Invoke(null, [_frameBody])!;
        var methodFrame = new AmqpMethodFrame(channel, decodedMethod);

        if (decodedMethod.HasBody())
        {
            if (!_partialFrames.ContainsKey(channel)) _partialFrames[channel] = new Queue<AmqpMethodFrame>();

            _partialFrames[channel].Enqueue(methodFrame);
            return null;
        }

        return methodFrame;
    }

    private AmqpHeaderFrame? HandleContentHeaderFrame(short channel)
    {
        using var reader = new BinReader(_frameBody);
        var classId = reader.ReadInt16();
        // Weight - reserved, ignore
        reader.ReadInt16();
        var bodyLength = reader.ReadInt64();
        var propsList = reader.ReadProperties();
        var lastPending = _partialFrames[channel].Peek();
        lastPending.BodyLength = bodyLength;
        lastPending.Properties = propsList;
        lastPending.Body = [];

        return null;
    }

    private AmqpMethodFrame? HandleBodyFrame(short channel)
    {
        var lastPending = _partialFrames[channel].Peek();
        if (lastPending.BodyLength == 0) return _partialFrames[channel].Dequeue();

        lastPending.Body = lastPending.Body.Concat(_frameBody).ToArray();
        if (lastPending.BodyLength > lastPending.Body.LongLength) return null;

        return _partialFrames[channel].Dequeue();
    }

    private async Task<FrameHeader> ReadFrameHeader(CancellationToken cancellationToken)
    {
        var header = await ReadAsync(FrameHeaderSize, cancellationToken);

        var type = header.Span[0];
        var channel = BinaryPrimitives.ReadInt16BigEndian(header.Span[1..3]);
        var size = BinaryPrimitives.ReadInt32BigEndian(header.Span[3..7]);
        return new FrameHeader(type, channel, size);
    }

    public async Task<Memory<byte>> ReadAsync(int size, CancellationToken cancellationToken)
    {
        Memory<byte> buffer = new byte[size];
        await _sourceStream.ReadExactlyAsync(buffer, cancellationToken);
        return buffer;
    }

    private record FrameHeader(byte Type, short Channel, int Size);

    public ValueTask DisposeAsync()
    {
        return _sourceStream.DisposeAsync();
    }

    public void Dispose()
    {
        _sourceStream.Dispose();
    }
}