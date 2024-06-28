using System.Collections.Concurrent;
using AMQPClient.Protocol;
using AMQPClient.Protocol.Methods;
namespace AMQPClient;

internal abstract class ChannelBase
{
    private readonly IAmqpFrameSender _frameSender;
    protected readonly ConcurrentQueue<TaskCompletionSource<MethodResult>> SyncMethodHandles = new();
    protected ChannelState State { get; set; }
    protected MethodResult? LastErrorResult { get; set; }
    protected bool IsOpen => State == ChannelState.Open;
    protected bool IsClosed => State == ChannelState.Closed;
    protected bool IsFailed => State == ChannelState.Failed;

    public ChannelBase(IAmqpFrameSender frameSender, short id)
    {
        _frameSender = frameSender;
        ChannelId = id;
    }

    public short ChannelId { get; }
    
    protected async Task<TResponse> CallMethodAsync<TResponse>(Method method, bool checkForClosed = true)
        where TResponse : Method, new()
    {
        var taskSource = new TaskCompletionSource<MethodResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        await CallMethodAsync(method, checkForClosed);
        SyncMethodHandles.Enqueue(taskSource);

        var result = await taskSource.Task;
        if (result.IsOk())
        {
            return (TResponse)result.Method!;
        }

        throw new Exception($"Method call failed with {result.ErrorCode} {result.ErrorMessage}");
    }

    protected Task CallMethodAsync(Method method, bool checkForClosed = true)
    {
        if (checkForClosed && IsClosed)
        {
            throw new ResourceClosedException();
        }

        if (IsFailed)
        {
            throw new ResourceClosedException(LastErrorResult!.ErrorCode, LastErrorResult.ErrorMessage);
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

internal enum ChannelState
{
    Open,
    Closed,
    Failed
}