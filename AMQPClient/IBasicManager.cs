namespace AMQPClient;

public interface IBasicManager
{
    public Task<string> BasicConsume(string queue, Action<IMessage> consumer);
    public Task BasicCancel(string consumerTag, bool noWait = false);
    public Task BasicPublishAsync(string exchange, string routingKey, IMessage message);
    public Task BasicAck(IMessage message);
    public Task BasicReject(IMessage message, bool requeue = false);
    public Task BasicRecover();
    public Task BasicQos(short prefetchCount, bool global = false);
    public Task<IMessage?> BasicGet(string queue, bool noAck = false);
}
