namespace AMQPClient.Protocol;

public interface IAmqpFrameSender
{
    Task SendFrameAsync(AmqpFrame frame);
}