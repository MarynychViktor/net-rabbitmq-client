namespace AMQPClient;

public interface IChannel
{
    public Task ExchangeDeclare(string name, bool passive = false, bool durable = false, bool autoDelete = false,
        bool internalOnly = false, bool nowait = false);
    // Not supported by rabbit-mq according to https://www.rabbitmq.com/docs/specification#methods
    public Task Flow(bool active);
    public Task Close();
    public Task ExchangeDelete(string name);
    public Task<string> QueueDeclare(string name = "");
    public Task QueueBind(string queue, string exchange, string routingKey);
    public Task QueueUnbind(string queue, string exchange, string routingKey, Dictionary<string, object>? arguments = null);
    public Task QueueDelete(string queue, bool ifUnused = false, bool ifEmpty = false, bool noWait = false);
    public Task<string> BasicConsume(string queue, Action<IMessage> consumer);
    public Task BasicCancel(string consumerTag, bool noWait = false);
    public Task BasicPublishAsync(string exchange, string routingKey, IMessage message);
    public Task BasicAck(IMessage message);
    public Task BasicReject(IMessage message, bool requeue = false);
}
