using AMQPClient.Methods;

namespace AMQPClient.Protocol;

public class LowLevelAmqpFrame
{
    public FrameType Type { get; protected set; }
    public short Channel { get; }
    // public byte[] Body { get; set; }
    public byte[] Payload { get; }

    public LowLevelAmqpFrame(short channel, byte[] payload, FrameType type)
    {
        Channel = channel;
        Payload = payload;
        Type = type;
    }

    public byte[] ToBytes()
    {
        var writer = new BinWriter();
        writer.Write((byte)1);
        writer.Write(Channel);
        writer.Write(Payload.Length);
        writer.Write(Payload);
        writer.Write((byte)0xCE);

        return writer.ToArray();
    }
}

public class LowLevelAmqpMethodFrame : LowLevelAmqpFrame
{
    public LowLevelAmqpHeaderFrame? HeaderFrame { get; set; }
    public byte[]? Body { get; set; }
    public Method Method { get; set; }
    
    public LowLevelAmqpMethodFrame(short channel, Method payload) : base(channel, new byte[]{}, FrameType.Method)
    {
        Method = payload;
    }
}

public enum FrameType
{
    Method = 1,
    ContentHeader = 2,
    Body = 3,
    Heartbeat = 8,
}