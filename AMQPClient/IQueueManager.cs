namespace AMQPClient;

public interface IQueueManager
{
    public Task<string> QueueDeclareAsync(string name = "", bool passive = true, bool durable = false,
        bool exclusive = false, bool autoDelete = false, bool noWait = false, Dictionary<string, object>? args = null);
    public Task QueueBindAsync(string queue, string exchange, string routingKey);
    public Task QueueUnbindAsync(string queue, string exchange, string routingKey, Dictionary<string, object>? arguments = null);
    public Task QueuePurgeAsync(string queue, bool noWait = false);
    public Task QueueDeleteAsync(string queue, bool ifUnused = false, bool ifEmpty = false, bool noWait = false);
    // Not supported by rabbitmq according to https://www.rabbitmq.com/docs/specification#methods
    // public Task Flow(bool active);
}
