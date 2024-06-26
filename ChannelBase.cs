using AMQPClient.Protocol;

namespace AMQPClient;

public abstract class ChannelBase : IAmqpChannel
{
    protected readonly InternalConnection _connection;

    public ChannelBase(InternalConnection connection, short id)
    {
        _connection = connection;
        ChannelId = id;
    }

    public short ChannelId { get; }
}