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
    public short ClassId { get; }
    public short MethodId { get; }

    public LowLevelAmqpMethodFrame(short channel, short classId, short methodId,  byte[] payload) : base(channel, payload, FrameType.Method)
    {
        ClassId = classId;
        MethodId = methodId;
    }

    public T castTo<T>()  where T: Method, new()
    {
        return Decoder.UnmarshalMethodFrame<T>(Payload);
    }
}

public enum FrameType
{
    Method = 1,
    ContentHeader = 2,
    Body = 3,
}