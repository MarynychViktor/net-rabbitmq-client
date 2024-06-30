using Microsoft.Extensions.Logging;

namespace AMQPClient;

public class DefaultLoggerFactory
{
    public static ILogger<T> CreateLogger<T>()
    {
        using var factory = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Debug);
            builder.AddFilter("AMQPClient", LogLevel.Warning);
            // builder.AddFilter(typeof(SystemChannel).FullName, LogLevel.Debug);
            builder.AddConsole();
        });
        return factory.CreateLogger<T>();
    }
}