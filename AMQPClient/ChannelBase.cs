using System.Collections.Concurrent;
using AMQPClient.Protocol;
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

    public Task HandleConnectionClosed()
    {
        State = ChannelState.Closed;
        return Task.CompletedTask;
    }

    protected async Task<TResponse> CallMethodAndUnwrapAsync<TResponse>(IFrameMethod method, bool checkForClosed = true)
        where TResponse : IFrameMethod
    {
        var taskSource = new TaskCompletionSource<MethodResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        await DispatchMethodAsync(method, checkForClosed);
        SyncMethodHandles.Enqueue(taskSource);

        var result = await taskSource.Task;
        if (result.IsOk())
        {
            return (TResponse)result.Method!;
        }

        throw new Exception($"Method call failed with {result.ErrorCode} {result.ErrorMessage}");
    }

    protected async Task<MethodResult> CallMethodAsync(IFrameMethod method, bool checkForClosed = true)
    {
        var taskSource = new TaskCompletionSource<MethodResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        await DispatchMethodAsync(method, checkForClosed);
        SyncMethodHandles.Enqueue(taskSource);

        var result = await taskSource.Task;
        if (result.IsOk())
        {
            return result;
        }

        throw new Exception($"Method call failed with {result.ErrorCode} {result.ErrorMessage}");
    }

    protected Task DispatchMethodAsync(IFrameMethod method, bool checkForClosed = true)
    {
        if (checkForClosed && IsClosed)
        {
            throw new ResourceClosedException();
        }

        if (IsFailed)
        {
            throw new ResourceClosedException(LastErrorResult!.ErrorCode, LastErrorResult.ErrorMessage);
        }
        var bytes = method.Serialize();
        return _frameSender.SendFrameAsync(new AmqpFrame(ChannelId, bytes, FrameType.Method));
    }
    
    protected async Task DispatchMethodAsync(IFrameMethod method, HeaderProperties properties, byte[]? body)
    {
        await DispatchMethodAsync(method);

        var headerFrame = new AmqpHeaderFrame(ChannelId, method.SourceClassId, body?.Length ?? 0, properties);
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