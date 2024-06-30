namespace AMQPClient;

public interface IConnection
{
    public Task<IChannel> CreateChannelAsync();
    public Task CloseAsync();
}