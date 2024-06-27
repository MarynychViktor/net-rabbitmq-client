using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading.Channels;
using AMQPClient.Protocol;
using AMQPClient.Protocol.Methods;
using AMQPClient.Protocol.Methods.Connection;
using Microsoft.Extensions.Logging;

namespace AMQPClient;

// FIXME: handle errors
public class InternalConnection
{
    private const short DefaultChannelId = 0;
    private readonly Dictionary<int, IAmqpChannel> _channels = new();
    private readonly ConcurrentDictionary<short, ChannelWriter<object>> _channelWriters = new();
    private readonly ConnectionParams _params;
    private AmqpFrameStream _amqpFrameStream;
    private int _channelId;
    private short _heartbeatInterval = 60;
    private long _lastFrameSent;
    private long _lastFrameReceived;
    private CancellationTokenSource _listenersCancellationSource;
    internal SystemChannel SystemChannel { get; set; }
    private ILogger<InternalConnection> Logger { get; } = DefaultLoggerFactory.CreateLogger<InternalConnection>();

    public InternalConnection(ConnectionParams @params)
    {
        _params = @params;
    }
    
    public async Task StartAsync()
    {
        _amqpFrameStream = CreateStreamReader();
        await HandshakeAsync();
        SystemChannel = CreateSystemChannel();
        _listenersCancellationSource = new CancellationTokenSource();
        StartIncomingFramesListener(_listenersCancellationSource.Token);
        StartHeartbeatFrameListener(_listenersCancellationSource.Token);
    }

    public async Task CloseAsync()
    {
        await _listenersCancellationSource.CancelAsync();
        await _amqpFrameStream.DisposeAsync();
        _amqpFrameStream = null;
    }

    private void StartHeartbeatFrameListener(CancellationToken cancellationToken = default)
    {
        Task.Run(async () =>
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var frequency = _heartbeatInterval / 2;
                var lastSent = DateTimeOffset.Now.ToUnixTimeSeconds() - _lastFrameSent;
                var secondsToNextFrame = Math.Min(frequency - lastSent, frequency);

                if (secondsToNextFrame <= 0)
                {
                    Logger.LogDebug("***Heartbeat frame sent***");
                    var heartbeatFrame = new byte[] { 8, 0, 0, 0, 0, 0, 0, 0xCE };
                    await _amqpFrameStream.SendRawAsync(heartbeatFrame);
                    await Task.Delay(TimeSpan.FromSeconds(frequency), cancellationToken);
                    continue;
                }


                await Task.Delay(TimeSpan.FromSeconds(secondsToNextFrame), cancellationToken);
            }
        }, cancellationToken);
    }

    private AmqpFrameStream CreateStreamReader()
    {
        var tcpClient = new TcpClient(_params.Host, _params.Port);
        var reader = new AmqpFrameStream(tcpClient.GetStream());
        reader.FrameSent += () =>
        {
            Interlocked.Exchange(ref _lastFrameSent, DateTimeOffset.Now.ToUnixTimeSeconds());
        };
        reader.FrameReceived += () =>
        {
            Interlocked.Exchange(ref _lastFrameReceived, DateTimeOffset.Now.ToUnixTimeSeconds());
        };

        return reader;
    }

    public async Task HandshakeAsync()
    {
        async Task<AmqpMethodFrame> ReadNextMethodFrame()
        {
            var nextFrame = await _amqpFrameStream.ReadFrameAsync();

            // Healthcheck frame check
            if (nextFrame.Type == FrameType.Method) return (AmqpMethodFrame)nextFrame;

            return (AmqpMethodFrame)await _amqpFrameStream.ReadFrameAsync();
        }

        async Task WriteMethodFrameAsync(Method method)
        {
            var bytes = Encoder.MarshalMethodFrame(method);
            await _amqpFrameStream.SendFrameAsync(new AmqpFrame(DefaultChannelId, bytes, FrameType.Method));
        }
        Logger.LogDebug("Handshake started");

        var protocolHeader = "AMQP"u8.ToArray().Concat(new byte[] { 0, 0, 9, 1 }).ToArray();
        await _amqpFrameStream.SendRawAsync(protocolHeader);

        var nextFrame = await ReadNextMethodFrame();
        // TODO: Do something
        var _startMethod = (StartMethod)nextFrame.Method;

        // FIXME: review with dynamic params
        var startOkMethod = new StartOkMethod
        {
            ClientProperties = new Dictionary<string, object>
            {
                { "product", LibraryDefaults.Product },
                { "platform", LibraryDefaults.Platform },
                { "copyright", LibraryDefaults.Copyright },
                { "information", LibraryDefaults.Information }
            },
            Mechanism = LibraryDefaults.AuthMechanism,
            Response = $"{'\x00'}{_params.User}{'\x00'}{_params.Password}",
            Locale = LibraryDefaults.Locale
        };
        await WriteMethodFrameAsync(startOkMethod);

        nextFrame = await ReadNextMethodFrame();
        var tuneMethod = (TuneMethod)nextFrame.Method;
        _heartbeatInterval = tuneMethod.Heartbeat;

        var tuneOkMethod = new TuneOkMethod
        {
            ChannelMax = tuneMethod.ChannelMax,
            Heartbeat = tuneMethod.Heartbeat,
            FrameMax = tuneMethod.FrameMax
        };
        await WriteMethodFrameAsync(tuneOkMethod);

        var openMethod = new OpenMethod
        {
            VirtualHost = _params.Vhost
        };
        await WriteMethodFrameAsync(openMethod);

        nextFrame = await ReadNextMethodFrame();
        // TODO: handle response?
        var _openOkMethod = (OpenOkMethod)nextFrame.Method;

        Logger.LogDebug("Handshake completed");
    }

    public async Task<IChannel> OpenChannelAsync()
    {
        var channel = CreateChannel();
        await channel.OpenAsync(channel.ChannelId);
        return channel;
    }

    private SystemChannel CreateSystemChannel()
    {
        var trxChannel = Channel.CreateUnbounded<object>();
        var channel = new SystemChannel(trxChannel, _amqpFrameStream, this);
        _channels.Add(channel.ChannelId, channel);
        _channelWriters[channel.ChannelId] = trxChannel.Writer;
        channel.StartListener();

        return channel;
    }

    private ChannelImpl CreateChannel(short? id = null)
    {
        var channelId = id ?? NextChannelId();
        var trxChannel = Channel.CreateUnbounded<object>();
        var channel = new ChannelImpl(trxChannel, _amqpFrameStream, channelId);
        _channels.Add(channelId, channel);
        _channelWriters[channelId] = trxChannel.Writer;

        return channel;
    }

    private void StartIncomingFramesListener(CancellationToken cancellationToken = default)
    {
        var listener = new IncomingFrameListener(_amqpFrameStream, _channels, _channelWriters);
        Task.Run(async () => await listener.StartAsync(cancellationToken), cancellationToken);
    }

    private short NextChannelId()
    {
        Interlocked.Increment(ref _channelId);
        return (short)_channelId;
    }
}