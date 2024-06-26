using Microsoft.Extensions.Logging;

namespace AMQPClient;

public class DefaultLoggerFactory
{
    public static ILogger<T> CreateLogger<T>()
    {
        using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
        return factory.CreateLogger<T>();
    }
}