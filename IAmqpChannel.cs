namespace AMQPClient;

public interface IAmqpChannel
{
    Task HandleFrameAsync(byte type, byte[] body);
}