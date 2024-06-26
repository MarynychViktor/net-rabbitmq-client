using AMQPClient.Protocol.Types;

namespace AMQPClient.Protocol;

public interface IAmqpChannel
{
    // Task HandleFrameAsync(AmqpMethodFrame frame);
    // Task HandleEnvelopeAsync(AmqpEnvelope envelope);
}

public interface IChannel
{
    public Task ExchangeDeclare(string name, bool passive = false, bool durable = false, bool autoDelete = false,
        bool internalOnly = false, bool nowait = false);

    public Task ExchangeDelete(string name);
    public Task<string> QueueDeclare(string name = "");
    public Task QueueBind(string queue, string exchange, string routingKey);
    public Task BasicConsume(string queue, Action<AmqpEnvelope> consumer);
    public Task BasicPublishAsync(string exchange, string routingKey, Message message);
    public Task BasicAck(AmqpEnvelope message);
}

public enum EventType
{
    MethodFrame,
    HeaderFrame,
    BodyFrame
}

public record InternalEvent<T>(EventType Type, T Event) where T : class;