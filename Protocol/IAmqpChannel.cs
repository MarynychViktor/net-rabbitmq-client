using AMQPClient.Protocol.Types;

namespace AMQPClient.Protocol;

public interface IAmqpChannel
{
    Task HandleFrameAsync(AmqpMethodFrame frame);
    Task HandleEnvelopeAsync(AmqpEnvelope envelope);
}

public interface IAmqpEventHandler
{
    Task HandleEvent<T>(InternalEvent<T> @event) where T : class;
}

public enum EventType
{
    MethodFrame,
    HeaderFrame,
    BodyFrame,
}

public record InternalEvent<T>(EventType Type, T Event) where T : class;

