using System.Buffers.Binary;
using AMQPClient.Methods;
using AMQPClient.Protocol;

namespace AMQPClient;

public class AmqpStreamWrapper : IDisposable, IAsyncDisposable
{
    private const byte FrameHeaderSize = 7;
    private readonly Stream _sourceStream;
    private byte[]? _frameBody;

    public AmqpStreamWrapper(Stream sourceStream)
    {
        _sourceStream = sourceStream;
    }

    public Task SendFrameAsync(LowLevelAmqpFrame frame)
    {
        return SendRawAsync(frame.ToBytes());
    }

    private readonly SemaphoreSlim _sendLock = new(1, 1);
    
    public async Task SendRawAsync(byte[] bytes)
    {
        await _sendLock.WaitAsync();

        try
        {
            await _sourceStream.WriteAsync(bytes);
        }
        finally
        {
            _sendLock.Release();
        }
    }
    
    public async Task<LowLevelAmqpFrame> ReadFrameAsync()
    {
        var (type, channel, size) = await ReadFrameHeader();
        _frameBody = (await ReadAsync(size)).ToArray();
        // Read EOF frame
        await ReadAsync(1);

        var frameType = (FrameType)type;

        return frameType switch
        {
            FrameType.Method => ReadMethodFrame(channel),
            FrameType.ContentHeader => ContentHeaderFrame(channel),
            FrameType.Body => BodyFrame(channel),
            _ => throw new Exception("not implemented")
        };
    }

    private LowLevelAmqpMethodFrame ReadMethodFrame(short channel)
    {
        var classId = BinaryPrimitives.ReadInt16BigEndian(_frameBody.AsSpan()[..2]);
        var methodId = BinaryPrimitives.ReadInt16BigEndian(_frameBody.AsSpan()[2..4]);
        var methodInfo = typeof(Decoder).GetMethod("CreateMethodFrame")!;
        var genericMethod = methodInfo.MakeGenericMethod(MethodMetaRegistry.GetMethodType(classId, methodId));
        var decodedMethod = (Method)genericMethod.Invoke(null, [_frameBody]);

        return new LowLevelAmqpMethodFrame(channel, decodedMethod);
    }

    private LowLevelAmqpHeaderFrame ContentHeaderFrame(short channel)
    {
        using var reader = new BinReader(_frameBody);
        var classId = reader.ReadInt16();
        var _weight = reader.ReadInt16();
        var bodyLength = reader.ReadInt64();
        var propBytes = reader.ReadBytes(_frameBody.Length - 12);
        var propsList = HeaderProperties.FromRaw(propBytes);

        return new LowLevelAmqpHeaderFrame(channel, classId, bodyLength, propsList);
    }

    private LowLevelAmqpBodyFrame BodyFrame(short channel)
    {
        return new LowLevelAmqpBodyFrame(channel, _frameBody);
    }

    private async Task<FrameHeader> ReadFrameHeader()
    {
        var header = await ReadAsync(FrameHeaderSize);

        byte type = header.Span[0];
        var channel = BinaryPrimitives.ReadInt16BigEndian(header.Span[1..3]);
        var size = BinaryPrimitives.ReadInt32BigEndian(header.Span[3..7]);
        return new FrameHeader(type, channel, size);
    }

    public async Task<Memory<byte>> ReadAsync(int size)
    {
        Memory<byte> buffer =  new byte[size];
        await _sourceStream.ReadExactlyAsync(buffer, CancellationToken.None);
        return buffer;
    }

    public void Dispose()
    {
        _sourceStream.Dispose();
    }

    public ValueTask DisposeAsync()
    {
        return _sourceStream.DisposeAsync();
    }

    record FrameHeader(byte Type, short Channel, int Size);
}