using AMQPClient.Protocol;

namespace AMQPClient;

public interface IAmqpFrameSender
{
    Task SendFrameAsync(AmqpFrame frame);
    Task SendRawAsync(byte[] bytes);
}