using AMQPClient.Protocol;
using AMQPClient.Protocol.Types;

namespace AMQPClient;

public abstract class ChannelBase : IAmqpChannel
{
    protected readonly InternalConnection _connection;
    public short ChannelId { get; }

    public ChannelBase(InternalConnection connection, short id)
    {
        _connection = connection;
        ChannelId = id;
    }
}