using AMQPClient.Protocol;

namespace AMQPClient;

public class Connection
{
    private readonly ConnectionParams _connectionParams;
    private InternalConnection _internalConnection;

    internal Connection(ConnectionParams connectionParams)
    {
        _connectionParams = connectionParams;
    }

    internal async Task StartAsync()
    {
        var internalConnection = new InternalConnection(_connectionParams);
        await internalConnection.StartAsync();
        _internalConnection = internalConnection;
    }

    public Task<InternalChannel> CreateChannelAsync()
    {
        return _internalConnection.OpenChannelAsync();
    }
}