namespace AMQPClient.Protocol;

public class AMQPFrame
{
    public AMQPFrameType Type { get; set; }
    public short Channel { get; set; }
    public byte[] Body { get; set; }

    public static AMQPFrame MethodFrame(short channel, byte[] body)
    {
        return new AMQPFrame
        {
            Type = AMQPFrameType.Method,
            Channel = channel,
            Body = body
        };
    }

    public byte[] ToBytes()
    {
        var writer = new BinWriter();
        writer.Write((byte)1);
        writer.WriteShort(Channel);
        writer.WriteInt(Body.Length);
        writer.Write(Body);
        writer.Write((byte)0xCE);

        return writer.ToArray();
    }
}

public enum AMQPFrameType
{
    Method = 1,
    ContentHeader = 2,
    Body = 3
}