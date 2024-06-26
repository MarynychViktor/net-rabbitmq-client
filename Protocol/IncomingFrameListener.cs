using System.Collections.Concurrent;
using System.Threading.Channels;
using AMQPClient.Protocol.Types;
using Microsoft.Extensions.Logging;

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
    private ILogger<IncomingFrameListener> Logger { get; } = DefaultLoggerFactory.CreateLogger<IncomingFrameListener>();

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
                Logger.LogDebug("Waiting for next frame");
                _frame = await _amqpFrameStream.ReadFrameAsync();

                // Handle incomplete frames
                if (_frame == null) continue;

                switch (_frame.Type)
                {
                    case FrameType.Method:
                        await HandleMethodFrame();
                        continue;
                    case FrameType.Heartbeat:
                        Logger.LogWarning("Heartbeat frame ignored");
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

    private async Task PublishMethod(AmqpMethodFrame methodFrame)
    {
        await _channelWriters[methodFrame.Channel].WriteAsync(methodFrame);
    }

    private async Task HandleMethodFrame()
    {
        var methodFrame = (AmqpMethodFrame)_frame;
        var method = methodFrame.Method;
        var (classId, methodId) = methodFrame.Method.ClassMethodId();
        Logger.LogInformation("Received method frame '{typeName}'", method.GetType().Name);

        if (method.HasBody())
        {
            await PublishMethod(methodFrame);
            return;
        }

        await PublishMethod(methodFrame);
    }
}