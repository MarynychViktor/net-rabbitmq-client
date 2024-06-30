namespace AMQPClient;

public interface IExchangeManager
{
    public Task ExchangeDeclare(string name, bool passive = false, bool durable = false, bool autoDelete = false,
        bool internalOnly = false, bool nowait = false, string type = "direct");
    public Task ExchangeDelete(string name);
}
