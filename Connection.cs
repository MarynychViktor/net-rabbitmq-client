namespace AMQPClient;

public class Connection
{
    private readonly InternalConnection _internalConnection;

    private Connection(InternalConnection internalConnection)
    {
        _internalConnection = internalConnection;
    }

    public static async Task<Connection> OpenAsync()
    {
        var internalConnection = new InternalConnection();
        await internalConnection.OpenAmqpConnectionAsync();

        return new Connection(internalConnection);
    }

    public Task<Channel> CreateChannelAsync()
    {
        return _internalConnection.OpenChannelAsync();
    }
}