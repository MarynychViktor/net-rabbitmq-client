namespace AMQPClient;

public class ConnectionFactory
{
    public async Task<Connection> CreateConnectionAsync(Action<ConnectionParams> builderDelegate)
    {
        var connParams = new ConnectionParams();
        builderDelegate(connParams);

        var connection = new Connection(connParams);
        await connection.StartAsync();

        return connection;
    }
}