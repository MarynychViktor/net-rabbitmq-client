using System.Threading.Channels;
using AMQPClient.Protocol;
using Microsoft.Extensions.Logging;

namespace AMQPClient;

// FIXME: handle errors
public class IncomingFrameListener
{
    private readonly AmqpFrameStream _amqpFrameStream;
    private readonly Dictionary<int, IAmqpChannel> _channels;
    private readonly IReadOnlyDictionary<short, ChannelWriter<object>> _channelWriters;
    private AmqpFrame? _frame;

    public IncomingFrameListener(
        AmqpFrameStream amqpFrameStream,
        Dictionary<int, IAmqpChannel> channels,
        IReadOnlyDictionary<short, ChannelWriter<object>> channelWriters
    )
    {
        _amqpFrameStream = amqpFrameStream;
        _channels = channels;
        _channelWriters = channelWriters;
    }

    private ILogger<IncomingFrameListener> Logger { get; } = DefaultLoggerFactory.CreateLogger<IncomingFrameListener>();

    internal async Task StartAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested) return;

                Logger.LogDebug("Waiting for next frame");
                _frame = await _amqpFrameStream.ReadFrameAsync(cancellationToken);

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
            Console.WriteLine("-------------------Listener exited");
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