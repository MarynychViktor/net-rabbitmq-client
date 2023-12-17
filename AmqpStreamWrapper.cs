using AMQPClient.Methods;
using AMQPClient.Protocol;

namespace AMQPClient;

public class AmqpStreamWrapper
{
    private const byte FRAME_HEADER_SIZE = 7;
    private Stream _sourceStream;

    public AmqpStreamWrapper(Stream sourceStream)
    {
        _sourceStream = sourceStream;
    }

    public async Task InvokeMethod(short channel, Method method)
    {
        // FIXME: if method has body, write it should send multiple raw frames
        var bytes = Encoder.MarshalMethodFrame(method);
        var frame = new LowLevelAmqpFrame(channel, bytes, FrameType.Method);
        
        await SendRawAsync(frame.ToBytes());
    }
    

    public async Task SendFrameAsync(LowLevelAmqpFrame frame)
    {
        await SendRawAsync(frame.ToBytes());
    }

    public async Task SendRawAsync(byte[] bytes)
    {
        await _sourceStream.WriteAsync(bytes);
    }

    public async Task<LowLevelAmqpFrame> ReadFrameAsync()
    {
        var (type, channel, size) = await ReadFrameHeader();
        var frameBody = await ReadAsync(size);
        // Read EOF frame
        await ReadAsync(1);

        if (type == 1)
        {
            var (classId, methodId) = MethodProps(frameBody[..4]);
            return new LowLevelAmqpMethodFrame(channel, classId, methodId, frameBody);
        }
        else
        {
            throw new Exception("failed");
        }
    }

    private (short classId, short methodId) MethodProps(byte [] body)
    {
        var reader = new BinReader(body);
        var classId = reader.ReadInt16();
        var methodId = reader.ReadInt16();

        return (classId, methodId);
    }

    private async Task<(byte Type, short Channel, int Size)> ReadFrameHeader()
    {
        var header = await ReadAsync(FRAME_HEADER_SIZE);

        var reader = new BinReader(header);
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

        // while (bytesRead < size)
        // {
        //     byte[] buffer = new byte[size];
        //     bytesRead += await _tcpClient.Client.ReceiveAsync(buffer, SocketFlags.None);
        //     result = result.Concat(buffer).ToArray();
        // }

        return buffer2;
    }
}