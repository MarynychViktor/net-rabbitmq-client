using AMQPClient.Protocol;
using AMQPClient.Protocol.Types;

namespace AMQPClient;

public abstract class ChannelBase : IAmqpChannel, IAmqpEventHandler
{
    protected readonly InternalConnection _connection;
    public short ChannelId { get; }

    public ChannelBase(InternalConnection connection, short id)
    {
        _connection = connection;
        ChannelId = id;
    }

    public abstract Task HandleFrameAsync(AmqpMethodFrame frame);
    public abstract Task HandleEnvelopeAsync(AmqpEnvelope envelope);
    public abstract Task HandleEvent<T>(InternalEvent<T> @event) where T : class;
}