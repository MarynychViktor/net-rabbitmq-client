namespace AMQPClient.Protocol;

public class AMQPFrame
{
    public AMQPFrameType Type { get; set; }
    public short Channel { get; set; }
    public byte[] Body { get; set; }

    public static AMQPFrame MethodFrame( short channel, byte[] body)
    {
        return new AMQPFrame()
        {
            Type = AMQPFrameType.Method,
            Channel = channel,
            Body = body
        };
    }

    public byte[] ToBytes()
    {
        var stream = new MemoryStream();
        var writer = new BinWriter(stream);
        writer.Write((byte)1);
        writer.Write(Channel);
        writer.Write(Body.Length);
        Console.WriteLine($"Size {Body.Length}");
        writer.Write(Body);
        writer.Write((byte)0xCE);

        return stream.ToArray();
    }
}

public enum AMQPFrameType
{
    Method = 1,
    ContentHeader = 2,
    Body = 3,
}