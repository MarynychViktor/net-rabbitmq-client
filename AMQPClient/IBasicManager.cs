namespace AMQPClient;

public interface IBasicManager
{
    public Task<string> BasicConsumeAsync(string queue, Action<IMessage> consumer);
    public Task BasicCancelAsync(string consumerTag, bool noWait = false);
    public Task BasicPublishAsync(string exchange, string routingKey, IMessage message);
    public Task BasicAckAsync(IMessage message);
    public Task BasicRejectAsync(IMessage message, bool requeue = false);
    public Task BasicRecoverAsync();
    public Task BasicQosAsync(short prefetchCount, bool global = false);
    public Task<IMessage?> BasicGetAsync(string queue, bool noAck = false);
}
