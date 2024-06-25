using AMQPClient.Protocol.Methods;

namespace AMQPClient.Protocol;

public interface IMethodCaller
{
    Task<TResponse> CallMethodAsync<TResponse>(short channelId, Method method) where TResponse : Method, new();
    Task CallMethodAsync(short channel, Method method);
}