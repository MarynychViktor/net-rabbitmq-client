namespace AMQPClient;

public interface IChannel : IQueueManager, IExchangeManager, IBasicManager
{
    public Task CloseAsync();
}
