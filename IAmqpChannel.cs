using AMQPClient.Protocol;

namespace AMQPClient;

public interface IAmqpChannel
{
    Task HandleFrameAsync(byte type, byte[] body);
    Task HandleFrameAsync(LowLevelAmqpMethodFrame frame);
    Task HandleMethodFrameAsync(byte[] body);
}