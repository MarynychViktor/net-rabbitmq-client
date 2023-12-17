namespace AMQPClient;

public class PublicConnection
{
    private readonly InternalConnection _internalConnection;

    private PublicConnection(InternalConnection internalConnection)
    {
        _internalConnection = internalConnection;
    }

    public static async Task<PublicConnection> OpenAsync()
    {
        var internalConnection = new InternalConnection();
        await internalConnection.OpenAmqpConnectionAsync();

        return new PublicConnection(internalConnection);
    }

    public Task<Channel> OpenChannelAsync()
    {
        return _internalConnection.OpenChannelAsync();
    }
}