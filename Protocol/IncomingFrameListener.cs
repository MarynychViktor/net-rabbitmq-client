using System.Collections.Concurrent;
using AMQPClient.Protocol.Types;

namespace AMQPClient.Protocol;

// FIXME: handle errors
public class IncomingFrameListener
{
    private readonly AmqpStreamWrapper _amqpStreamWrapper;
    private readonly Dictionary<int, IAmqpChannel> _channels;
    private Dictionary<short, Queue<(LowLevelAmqpMethodFrame method, LowLevelAmqpHeaderFrame? header, byte[]? bodyFrame)>> _pendingFrames = new();
    private Dictionary<short, ConcurrentQueue<TaskCompletionSource<LowLevelAmqpMethodFrame>>> _methodWaitQueue;
    private LowLevelAmqpFrame _frame;

    public IncomingFrameListener(
        AmqpStreamWrapper amqpStreamWrapper,
        Dictionary<int, IAmqpChannel> channels,
        Dictionary<short, ConcurrentQueue<TaskCompletionSource<LowLevelAmqpMethodFrame>>> methodWaitQueue
    ) {
        _amqpStreamWrapper = amqpStreamWrapper;
        _channels = channels;
        _methodWaitQueue = methodWaitQueue;
    }

    private async Task ReadNextAsync()
    {
        _frame = await _amqpStreamWrapper.ReadFrameAsync();
    }

    private void HandleMethodFrame()
    {
        var methodFrame = (LowLevelAmqpMethodFrame)_frame;
        var (classId, methodId) = methodFrame.Method.ClassMethodId();
        Console.WriteLine($"Received method {classId} {methodId}");

        if (MethodMetaRegistry.HasBody(classId, methodId))
        {
            Queue<(LowLevelAmqpMethodFrame method, LowLevelAmqpHeaderFrame? header, byte[]? bodyFrame)> pendingChannelFrames;
            if (!_pendingFrames.TryGetValue(methodFrame.Channel, out pendingChannelFrames))
            {
                pendingChannelFrames = new();
                _pendingFrames[methodFrame.Channel] = pendingChannelFrames;
            }

            pendingChannelFrames.Enqueue((methodFrame, null, null));
            return;
        }

        ConcurrentQueue<TaskCompletionSource<LowLevelAmqpMethodFrame>> responseWaitersQueue;
        TaskCompletionSource<LowLevelAmqpMethodFrame> methodWaiter;

        if (
            MethodMetaRegistry.IsAsyncResponse(classId, methodId) &&
            _methodWaitQueue.TryGetValue(methodFrame.Channel, out responseWaitersQueue) &&
            responseWaitersQueue.TryDequeue(out methodWaiter)
        )
        {
            methodWaiter.SetResult(methodFrame);
            return;
        }
        else
        {
            Console.WriteLine($"Frame received {methodFrame.Method}");
            _channels[methodFrame.Channel].HandleFrameAsync(methodFrame);
        }

        throw new NotImplementedException("Not impelemented part");
    }

    private void HandleContentHeaderFrame()
    {
        var headerFrame = (LowLevelAmqpHeaderFrame)_frame;
        var lastMethodWoHeader = _pendingFrames[headerFrame.Channel].Dequeue();
        lastMethodWoHeader.header = headerFrame;
        _pendingFrames[headerFrame.Channel].Enqueue(lastMethodWoHeader);
    }

    private void HandleBodyFrame()
    {
        var bodyFrame = (LowLevelAmqpBodyFrame)_frame;
        var pendingFrame = _pendingFrames[bodyFrame.Channel].Dequeue();

        if (pendingFrame.bodyFrame == null)
        {
            pendingFrame.bodyFrame = bodyFrame.Payload;
        }
        else
        {
            pendingFrame.bodyFrame = pendingFrame.bodyFrame.Concat(bodyFrame.Payload).ToArray();
        }

        if (pendingFrame.header.BodyLength > pendingFrame.bodyFrame.Length)
        {
            _pendingFrames[bodyFrame.Channel].Enqueue(pendingFrame);
            return;
        }

        var envelopePayload = new AmqpEnvelopePayload(
            pendingFrame.header.Properties,
            pendingFrame.bodyFrame
        );

        var envelope = new AmqpEnvelope(pendingFrame.method.Method, envelopePayload);
        _channels[bodyFrame.Channel].HandleEnvelopeAsync(envelope);
    }
    
    internal async Task StartAsync()
    {
        try
        {
            // FIXME: cancellation
            while (true)
            {
                // var frame = await _amqpStreamWrapper.ReadFrameAsync();
                await ReadNextAsync();

                switch (_frame.Type)
                {
                    case FrameType.Method:
                        HandleMethodFrame();
                        continue;
                    case FrameType.ContentHeader:
                        HandleContentHeaderFrame();
                        continue;
                    case FrameType.Body:
                        HandleBodyFrame();
                        continue;
                    case FrameType.Heartbeat:
                        continue;
                    default:
                        throw new Exception($"Not matched type {_frame.Type}");
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"-------------------Listener failed with : {e}");
            throw;
        }
        finally
        {
            Console.WriteLine($"-------------------Listener exited");
        }
    }
}