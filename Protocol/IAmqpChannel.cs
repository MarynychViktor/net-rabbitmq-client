using AMQPClient.Protocol.Types;

namespace AMQPClient.Protocol;

public interface IAmqpChannel
{
    Task HandleFrameAsync(LowLevelAmqpMethodFrame frame);
    Task HandleEnvelopeAsync(AmqpEnvelope envelope);
}