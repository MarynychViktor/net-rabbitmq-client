using AMQPClient.Protocol.Methods;

namespace AMQPClient.Protocol;

public class AmqpFrame
{
    public FrameType Type { get; protected set; }
    public short Channel { get; }
    // public byte[] Body { get; set; }
    public byte[] Payload { get; }

    public AmqpFrame(short channel, byte[] payload, FrameType type)
    {
        Channel = channel;
        Payload = payload;
        Type = type;
    }

    public virtual byte[] ToBytes()
    {
        var writer = new BinWriter();
        writer.Write((byte)Type);
        writer.Write(Channel);
        writer.Write(Payload.Length);
        writer.Write(Payload);
        writer.Write((byte)0xCE);

        return writer.ToArray();
    }
}

public class AmqpMethodFrame : AmqpFrame
{
    public Method Method { get; set; }
    public HeaderProperties? Properties { get; set; }
    public long? BodyLength { get; set; }
    public byte[]? Body { get; set; }

    public AmqpMethodFrame(short channel, Method payload) : base(channel, new byte[]{}, FrameType.Method)
    {
        Method = payload;
    }
    
    
    public override byte[] ToBytes()
    {
        var writer = new BinWriter();
        writer.Write((byte)Type);
        writer.Write(Channel);
        var payload = Encoder.MarshalMethodFrame(Method);
        writer.Write(payload.Length);
        writer.Write(payload);
        writer.Write((byte)0xCE);

        return writer.ToArray();
    }
}

public enum FrameType
{
    Method = 1,
    ContentHeader = 2,
    Body = 3,
    Heartbeat = 8,
}