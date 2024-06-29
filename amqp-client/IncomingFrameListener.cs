using System.Threading.Channels;
using AMQPClient.Protocol;
using Microsoft.Extensions.Logging;

namespace AMQPClient;

// FIXME: handle errors
public class IncomingFrameListener
{
    private readonly AmqpFrameStream _amqpFrameStream;
    private readonly IReadOnlyDictionary<short, ChannelWriter<object>> _channelWriters;
    private AmqpFrame? _frame;

    public IncomingFrameListener(
        AmqpFrameStream amqpFrameStream,
        IReadOnlyDictionary<short, ChannelWriter<object>> channelWriters
    )
    {
        _amqpFrameStream = amqpFrameStream;
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
        catch (OperationCanceledException e)
        {
            Logger.LogWarning("Frame loop cancelled");
        } catch (Exception e)
        {
            Logger.LogError("Frame loop failed with {msg}", e.ToString());
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
        Logger.LogDebug("Received method frame:\n\t {method}", method);

        if (method.HasBody())
        {
            await PublishMethod(methodFrame);
            return;
        }

        await PublishMethod(methodFrame);
    }
}