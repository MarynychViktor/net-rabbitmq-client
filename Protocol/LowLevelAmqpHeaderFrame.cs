using AMQPClient.Methods;

namespace AMQPClient.Protocol;


public class LowLevelAmqpHeaderFrame : LowLevelAmqpFrame
{
    public short ClassId { get; }
    public long BodyLength { get; }
    public HeaderProperties Properties { get; }

    public LowLevelAmqpHeaderFrame(short channel, short classId, long bodyLength,  HeaderProperties properties) : base(channel, new byte[] {}, FrameType.ContentHeader)
    {
        BodyLength = bodyLength;
        ClassId = classId;
        Properties = properties;
    }
}
