namespace AMQPClient;

public class ConnectionFactory
{
    public async Task<IConnection> CreateConnectionAsync(Action<ConnectionParams> builderDelegate)
    {
        var connParams = new ConnectionParams();
        builderDelegate(connParams);
        var connection = new ConnectionImpl(connParams);
        await connection.StartAsync();

        return connection;
    }
}