using AMQPClient.Protocol;
using AMQPClient.Types;

namespace AMQPClient;

public interface IAmqpChannel
{
    Task HandleFrameAsync(LowLevelAmqpMethodFrame frame);
    Task HandleEnvelopeAsync(AmqpEnvelope envelope);
}