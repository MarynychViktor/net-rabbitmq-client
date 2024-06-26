namespace AMQPClient.Protocol;

public class AmqpBodyFrame : AmqpFrame
{
    public AmqpBodyFrame(short channel, byte[] payload) : base(channel, payload, FrameType.Body)
    {
    }
}