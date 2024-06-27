using System.Collections.Concurrent;
using AMQPClient.Protocol;
using AMQPClient.Protocol.Methods;
namespace AMQPClient;

public abstract class ChannelBase : IAmqpChannel
{
    protected readonly IAmqpFrameSender FrameSender;
    protected readonly Dictionary<short, ConcurrentQueue<TaskCompletionSource<AmqpMethodFrame>>> SyncMethodHandles =
        new();
    protected bool IsClosed { get; set; }

    public ChannelBase(IAmqpFrameSender frameSender, short id)
    {
        FrameSender = frameSender;
        ChannelId = id;
    }

    public short ChannelId { get; }
    
    protected async Task<TResponse> CallMethodAsync<TResponse>(short channelId, Method method, bool checkForClosed = true)
        where TResponse : Method, new()
    {
        var taskSource = new TaskCompletionSource<AmqpMethodFrame>(TaskCreationOptions.RunContinuationsAsynchronously);
        await CallMethodAsync(channelId, method, checkForClosed);

        if (!SyncMethodHandles.TryGetValue(channelId, out var sourcesQueue))
        {
            sourcesQueue = new ConcurrentQueue<TaskCompletionSource<AmqpMethodFrame>>();
            SyncMethodHandles[channelId] = sourcesQueue;
        }

        sourcesQueue.Enqueue(taskSource);

        return (TResponse)(await taskSource.Task).Method;
    }

    protected Task CallMethodAsync(short channel, Method method, bool checkForClosed = true)
    {
        if (checkForClosed && IsClosed)
        {
            throw new Exception("Unable to call method on closed channel");
        }

        var bytes = Encoder.MarshalMethodFrame(method);
        return FrameSender.SendFrameAsync(new AmqpFrame(channel, bytes, FrameType.Method));
    }
    
    protected async Task CallMethodAsync(short channel, Method method, HeaderProperties properties, byte[]? body)
    {
        await CallMethodAsync(channel, method);

        var headerFrame = new AmqpHeaderFrame(channel, method.ClassId, body?.Length ?? 0, properties);
        await FrameSender.SendFrameAsync(headerFrame);

        var bodyFrame = new AmqpBodyFrame(channel, body ?? []);
        await FrameSender.SendFrameAsync(bodyFrame);
    }
}