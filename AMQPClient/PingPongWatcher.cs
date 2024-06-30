using Microsoft.Extensions.Logging;

namespace AMQPClient;

public class PingPongWatcher
{
    public short HeartbeatInterval { get; set; } = 60;
    private long _lastFrameReceived;
    private long _lastFrameSent;
    public long LastFrameSent
    {
        get => _lastFrameSent;
        set => Interlocked.Exchange(ref _lastFrameSent, value);
    }
    public long LastFrameReceived
    {
        get => _lastFrameReceived;
        set => Interlocked.Exchange(ref _lastFrameReceived, value);
    }
    private ILogger<InternalConnection> _logger = DefaultLoggerFactory.CreateLogger<InternalConnection>();
    private readonly IAmqpFrameSender _frameSender;

    public PingPongWatcher(IAmqpFrameSender frameSender)
    {
        _frameSender = frameSender;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        while (true)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            var frequency = HeartbeatInterval / 2;
            var lastSent = DateTimeOffset.Now.ToUnixTimeSeconds() - _lastFrameSent;
            var secondsToNextFrame = Math.Min(frequency - lastSent, frequency);

            if (secondsToNextFrame <= 0)
            {
                _logger.LogDebug("***Heartbeat frame sent***");
                var heartbeatFrame = new byte[] { 8, 0, 0, 0, 0, 0, 0, 0xCE };
                await _frameSender.SendRawAsync(heartbeatFrame);
                await Task.Delay(TimeSpan.FromSeconds(frequency), cancellationToken);
                continue;
            }


            await Task.Delay(TimeSpan.FromSeconds(secondsToNextFrame), cancellationToken);
        }
    }

}