using System.Collections.Concurrent;
using System.Threading.Channels;
using AMQPClient.Protocol.Methods;

namespace AMQPClient.Protocol;

public class MethodCaller(AmqpFrameStream amqpFrameStream, Dictionary<short, ConcurrentQueue<TaskCompletionSource<AmqpMethodFrame>>> syncCallResponseWaitQueue) : IMethodCaller
{
    public Dictionary<short, ConcurrentQueue<TaskCompletionSource<AmqpMethodFrame>>> SyncCallResponseWaitQueue =>
        syncCallResponseWaitQueue;

    public async Task<TResponse> CallMethodAsync<TResponse>(short channelId, Method method) where TResponse : Method, new()
    {
        var taskSource = new TaskCompletionSource<AmqpMethodFrame>(TaskCreationOptions.RunContinuationsAsynchronously);

        await CallMethodAsync(channelId, method);

        if (!syncCallResponseWaitQueue.TryGetValue(channelId, out ConcurrentQueue<TaskCompletionSource<AmqpMethodFrame>>? sourcesQueue))
        {
            sourcesQueue = new();
            syncCallResponseWaitQueue[channelId] = sourcesQueue;
        }

        sourcesQueue.Enqueue(taskSource);

        return (TResponse)(await taskSource.Task).Method;
    }

    public Task CallMethodAsync(short channel, Method method)
    {
        var bytes = Encoder.MarshalMethodFrame(method);
        return amqpFrameStream.SendFrameAsync(new AmqpFrame(channel, bytes, FrameType.Method));
    }
}
