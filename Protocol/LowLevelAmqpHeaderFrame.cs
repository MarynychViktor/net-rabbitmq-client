using AMQPClient.Methods;

namespace AMQPClient.Protocol;


public class LowLevelAmqpHeaderFrame : LowLevelAmqpFrame
{
    public short ClassId { get; }
    public long BodyLength { get; }

    public LowLevelAmqpHeaderFrame(short channel, short classId, long bodyLength,  byte[] payload) : base(channel, payload, FrameType.ContentHeader)
    {
        BodyLength = bodyLength;
        ClassId = classId;
    }
}
