using System.Buffers.Binary;
using AMQPClient.Methods;
using AMQPClient.Protocol;

namespace AMQPClient;

public class AmqpStreamWrapper : IDisposable, IAsyncDisposable
{
    private const byte FRAME_HEADER_SIZE = 7;
    private Stream _sourceStream;

    public AmqpStreamWrapper(Stream sourceStream)
    {
        _sourceStream = sourceStream;
    }

    public Task SendFrameAsync(LowLevelAmqpFrame frame)
    {
        return SendRawAsync(frame.ToBytes());
    }

    public async Task SendRawAsync(byte[] bytes)
    {
        await _sourceStream.WriteAsync(bytes);
    }

    public async Task<LowLevelAmqpFrame> ReadFrameAsync()
    {
        Console.WriteLine($"Before read header");
        var (type, channel, size) = await ReadFrameHeader();
        var frameBody = await ReadAsync(size);
        // Read EOF frame
        await ReadAsync(1);

        var frameType = (FrameType)type;

        if (frameType == FrameType.Method)
        {
            var classId = BinaryPrimitives.ReadInt16BigEndian(frameBody.AsSpan()[..2]);
            var methodId = BinaryPrimitives.ReadInt16BigEndian(frameBody.AsSpan()[2..4]);
            var methodInfo = typeof(Decoder).GetMethod("CreateMethodFrame")!;
            var genericMethod = methodInfo.MakeGenericMethod(MethodMetaRegistry.GetMethodType(classId, methodId));
            var decodedMethod = (Method)genericMethod.Invoke(null, [frameBody]);

            return new LowLevelAmqpMethodFrame(channel, decodedMethod);
        }

        if (frameType == FrameType.ContentHeader)
        {
            using var reader = new BinReader(frameBody);
            var classId = reader.ReadInt16();
            var _weight = reader.ReadInt16();
            var bodyLength = reader.ReadInt64();
            var propBytes = reader.ReadBytes(frameBody.Length - 12);
            var propsList = HeaderProperties.FromRaw(propBytes);

            return new LowLevelAmqpHeaderFrame(channel, classId, bodyLength, propsList);
        }

        if (frameType == FrameType.Body)
        {
            return new LowLevelAmqpBodyFrame(channel, frameBody);
        }

        throw new Exception("failed");
    }

    private async Task<(byte Type, short Channel, int Size)> ReadFrameHeader()
    {
        var header = await ReadAsync(FRAME_HEADER_SIZE);

        using var reader = new BinReader(header);
        var type = reader.ReadByte();
        var channel = reader.ReadInt16();
        var size = reader.ReadInt32();

        return (type, channel, size);
    }

    public async Task<byte[]> ReadAsync(int size)
    {
        int bytesRead = 0;
        byte[] buffer2 = new byte[size];

        while (bytesRead < size)
        {
            var read = await _sourceStream.ReadAsync(buffer2, bytesRead, size - bytesRead, CancellationToken.None);

            if (read == 0)
            {
                // TODO: handle close case
            }

            bytesRead += read;
        }

        return buffer2;
    }

    public void Dispose()
    {
        _sourceStream.Dispose();
    }

    public ValueTask DisposeAsync()
    {
        return _sourceStream.DisposeAsync();
    }
}