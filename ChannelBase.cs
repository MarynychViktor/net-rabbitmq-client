using System.Collections.Concurrent;
using AMQPClient.Protocol;
using AMQPClient.Protocol.Methods;
namespace AMQPClient;

public abstract class ChannelBase : IAmqpChannel
{
    private readonly IAmqpFrameSender _frameSender;
    protected readonly Dictionary<short, ConcurrentQueue<TaskCompletionSource<AmqpMethodFrame>>> SyncMethodHandles =
        new();
    protected bool IsClosed { get; set; }

    public ChannelBase(IAmqpFrameSender frameSender, short id)
    {
        _frameSender = frameSender;
        ChannelId = id;
    }

    public short ChannelId { get; }
    
    protected async Task<TResponse> CallMethodAsync<TResponse>(Method method, bool checkForClosed = true)
        where TResponse : Method, new()
    {
        var taskSource = new TaskCompletionSource<AmqpMethodFrame>(TaskCreationOptions.RunContinuationsAsynchronously);
        await CallMethodAsync(method, checkForClosed);

        if (!SyncMethodHandles.TryGetValue(ChannelId, out var sourcesQueue))
        {
            sourcesQueue = new ConcurrentQueue<TaskCompletionSource<AmqpMethodFrame>>();
            SyncMethodHandles[ChannelId] = sourcesQueue;
        }

        sourcesQueue.Enqueue(taskSource);

        return (TResponse)(await taskSource.Task).Method;
    }

    protected Task CallMethodAsync(Method method, bool checkForClosed = true)
    {
        if (checkForClosed && IsClosed)
        {
            throw new Exception("Unable to call method on closed channel");
        }

        var bytes = Encoder.MarshalMethodFrame(method);
        return _frameSender.SendFrameAsync(new AmqpFrame(ChannelId, bytes, FrameType.Method));
    }
    
    protected async Task CallMethodAsync(Method method, HeaderProperties properties, byte[]? body)
    {
        await CallMethodAsync(method);

        var headerFrame = new AmqpHeaderFrame(ChannelId, method.ClassId, body?.Length ?? 0, properties);
        await _frameSender.SendFrameAsync(headerFrame);

        var bodyFrame = new AmqpBodyFrame(ChannelId, body ?? []);
        await _frameSender.SendFrameAsync(bodyFrame);
    }
}