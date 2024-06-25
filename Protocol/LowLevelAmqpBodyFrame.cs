namespace AMQPClient.Protocol;


public class LowLevelAmqpBodyFrame : LowLevelAmqpFrame
{
    public LowLevelAmqpBodyFrame(short channel, byte[] payload) : base(channel, payload, FrameType.Body)
    {
    }
}
