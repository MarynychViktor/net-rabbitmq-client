namespace AMQPClient;

public interface IAmqpChannel
{
    // Task HandleFrameAsync(AmqpMethodFrame frame);
    // Task HandleEnvelopeAsync(AmqpEnvelope envelope);
}

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
    public Task BasicConsume(string queue, Action<IMessage> consumer);
    public Task BasicPublishAsync(string exchange, string routingKey, IMessage message);
    public Task BasicAck(IMessage message);
}
