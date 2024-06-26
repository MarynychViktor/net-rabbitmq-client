using System.Collections.Concurrent;
using System.Threading.Channels;
using AMQPClient.Protocol.Types;

namespace AMQPClient.Protocol;

// FIXME: handle errors
public class IncomingFrameListener
{
    private readonly AmqpFrameStream _amqpFrameStream;
    private readonly Dictionary<int, IAmqpChannel> _channels;
    private Dictionary<short, Queue<(AmqpMethodFrame method, AmqpHeaderFrame? header, byte[]? bodyFrame)>> _pendingFrames = new();
    private Dictionary<short, ConcurrentQueue<TaskCompletionSource<AmqpMethodFrame>>> _methodWaitQueue;
    private readonly IReadOnlyDictionary<short, ChannelWriter<object>> _channelWriters;
    private AmqpFrame _frame;

    public IncomingFrameListener(
        AmqpFrameStream amqpFrameStream,
        Dictionary<int, IAmqpChannel> channels,
        Dictionary<short, ConcurrentQueue<TaskCompletionSource<AmqpMethodFrame>>> methodWaitQueue,
        IReadOnlyDictionary<short, ChannelWriter<object>> channelWriters
    ) {
        _amqpFrameStream = amqpFrameStream;
        _channels = channels;
        _methodWaitQueue = methodWaitQueue;
        _channelWriters = channelWriters;
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
                
                // Handle incomplete frames
                if (_frame == null) continue;

                switch (_frame.Type)
                {
                    case FrameType.Method:
                        await HandleMethodFrame();
                        continue;
                    case FrameType.ContentHeader:
                        throw new Exception($"Not expected type {_frame.Type}");
                        continue;
                    case FrameType.Body:
                        throw new Exception($"Not expected type {_frame.Type}");
                        continue;
                    case FrameType.Heartbeat:
                        Console.WriteLine("FrameType.Heartbeat received ----");
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

    private async Task ReadNextAsync()
    {
        _frame = await _amqpFrameStream.ReadFrameAsync();
    }

    private async Task PublishMethod(AmqpMethodFrame methodFrame)
    {
        await _channelWriters[methodFrame.Channel].WriteAsync(methodFrame);
    }

    private async Task HandleMethodFrame()
    {
        var methodFrame = (AmqpMethodFrame)_frame;
        var method = methodFrame.Method;
        var (classId, methodId) = methodFrame.Method.ClassMethodId();
        Console.WriteLine($"Received method {classId} {methodId}");

        if (method.HasBody())
        {
            
            var envelopePayload = new AmqpEnvelopePayload(
                methodFrame.Properties,
                methodFrame.Body
            );

            var envelope = new AmqpEnvelope(method, envelopePayload);
            await PublishMethod(methodFrame);
            // _channels[methodFrame.Channel].HandleEnvelopeAsync(envelope);
            return;
        }
        // if (method.HasBody())
        // {
        //     Queue<(AmqpMethodFrame method, AmqpHeaderFrame? header, byte[]? bodyFrame)> pendingChannelFrames;
        //     if (!_pendingFrames.TryGetValue(methodFrame.Channel, out pendingChannelFrames))
        //     {
        //         pendingChannelFrames = new();
        //         _pendingFrames[methodFrame.Channel] = pendingChannelFrames;
        //     }
        //
        //     pendingChannelFrames.Enqueue((methodFrame, null, null));
        //     return;
        // }
        //
        ConcurrentQueue<TaskCompletionSource<AmqpMethodFrame>> responseWaitersQueue;
        TaskCompletionSource<AmqpMethodFrame> methodWaiter;

        // if (
            // method.IsAsyncResponse() &&
            // _methodWaitQueue.TryGetValue(methodFrame.Channel, out responseWaitersQueue) &&
            // responseWaitersQueue.TryDequeue(out methodWaiter)
        // )
        // {
            // methodWaiter.SetResult(methodFrame);
            Console.WriteLine("before method sent");
            await PublishMethod(methodFrame);
            Console.WriteLine("method sent");
            return;
        // }
        // else
        // {
            // Console.WriteLine($"Frame received {methodFrame.Method}");
            // _channels[methodFrame.Channel].HandleFrameAsync(methodFrame);
        // }

        // throw new NotImplementedException("Not impelemented part");
    }

    // private void HandleContentHeaderFrame()
    // {
    //     var headerFrame = (AmqpHeaderFrame)_frame;
    //     var lastMethodWoHeader = _pendingFrames[headerFrame.Channel].Dequeue();
    //     lastMethodWoHeader.header = headerFrame;
    //     _pendingFrames[headerFrame.Channel].Enqueue(lastMethodWoHeader);
    // }

    // private void HandleBodyFrame()
    // {
    //     var bodyFrame = (AmqpBodyFrame)_frame;
    //     var pendingFrame = _pendingFrames[bodyFrame.Channel].Dequeue();
    //
    //     if (pendingFrame.bodyFrame == null)
    //     {
    //         pendingFrame.bodyFrame = bodyFrame.Payload;
    //     }
    //     else
    //     {
    //         pendingFrame.bodyFrame = pendingFrame.bodyFrame.Concat(bodyFrame.Payload).ToArray();
    //     }
    //
    //     if (pendingFrame.header.BodyLength != 0 && pendingFrame.header.BodyLength > pendingFrame.bodyFrame.Length)
    //     {
    //         _pendingFrames[bodyFrame.Channel].Enqueue(pendingFrame);
    //         return;
    //     }
    //
    //     var envelopePayload = new AmqpEnvelopePayload(
    //         pendingFrame.header.Properties,
    //         pendingFrame.bodyFrame
    //     );
    //
    //     var envelope = new AmqpEnvelope(pendingFrame.method.Method, envelopePayload);
    //     _channels[bodyFrame.Channel].HandleEnvelopeAsync(envelope);
    // }
}