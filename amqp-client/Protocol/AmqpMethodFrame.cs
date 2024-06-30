namespace AMQPClient.Protocol;

public class AmqpFrame
{
    public AmqpFrame(short channel, byte[] payload, FrameType type)
    {
        Channel = channel;
        Payload = payload;
        Type = type;
    }

    public FrameType Type { get; protected set; }

    public short Channel { get; }

    // public byte[] Body { get; set; }
    public byte[] Payload { get; }

    public virtual byte[] ToBytes()
    {
        var writer = new BinWriter();
        writer.Write((byte)Type);
        writer.WriteShort(Channel);
        writer.WriteInt(Payload.Length);
        writer.Write(Payload);
        writer.Write((byte)0xCE);

        return writer.ToArray();
    }
}

public class AmqpMethodFrame : AmqpFrame
{
    public AmqpMethodFrame(short channel, IFrameMethod payload) : base(channel, new byte[] { }, FrameType.Method)
    {
        Method = payload;
    }

    public IFrameMethod Method { get; set; }
    public HeaderProperties? Properties { get; set; }
    public long? BodyLength { get; set; }
    public byte[]? Body { get; set; }


    public override byte[] ToBytes()
    {
        var writer = new BinWriter();
        writer.Write((byte)Type);
        writer.WriteShort(Channel);
        var payload = Method.Serialize();
        writer.WriteInt(payload.Length);
        writer.Write(payload);
        writer.Write((byte)0xCE);

        return writer.ToArray();
    }
}

public class AmqpMethodFrame2
{
    public AmqpMethodFrame2(short channel, IFrameMethod method, HeaderProperties? properties = null, long? bodyLength = null, byte[]? body = null)
    {
        Channel = channel;
        Method = method;
        Properties = properties;
        BodyLength = bodyLength;
        Body = body;
    }

    public short Channel { get; set; }
    public IFrameMethod Method { get; set; }
    public HeaderProperties? Properties { get; set; }
    public long? BodyLength { get; set; }
    public byte[]? Body { get; set; }


    public byte[] ToBytes()
    {
        var writer = new BinWriter();
        writer.Write((byte)1);
        writer.WriteShort(Channel);
        var payload = Method.Serialize();
        writer.WriteInt(payload.Length);
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
    Heartbeat = 8
}