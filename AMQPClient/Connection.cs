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

    public async Task CloseAsync()
    {
        await _internalConnection.SystemChannel.CloseConnection();
    }

    public Task<IChannel> CreateChannelAsync()
    {
        return _internalConnection.OpenChannelAsync();
    }
}