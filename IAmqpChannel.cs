using AMQPClient.Protocol;

namespace AMQPClient;

public interface IAmqpChannel
{
    Task HandleFrameAsync(LowLevelAmqpMethodFrame frame);
}