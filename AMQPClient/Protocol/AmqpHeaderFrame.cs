namespace AMQPClient.Protocol;

public class AmqpHeaderFrame : AmqpFrame
{
    public AmqpHeaderFrame(short channel, short classId, long bodyLength, HeaderProperties properties) : base(channel,
        new byte[] { }, FrameType.ContentHeader)
    {
        BodyLength = bodyLength;
        ClassId = classId;
        Properties = properties;
    }

    public short ClassId { get; }
    public long BodyLength { get; }
    public HeaderProperties Properties { get; }

    public override byte[] ToBytes()
    {
        var payload = Properties.ToRaw();
        var writer = new BinWriter();
        writer.Write((byte)Type);
        writer.WriteShort(Channel);
        writer.WriteInt(payload.Length + 12);
        writer.WriteShort(ClassId);
        writer.WriteShort((short)0);
        writer.WriteLong(BodyLength);
        writer.Write(payload);
        writer.Write((byte)0xCE);

        return writer.ToArray();
    }
}