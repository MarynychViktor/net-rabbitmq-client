using System.Collections.Concurrent;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading.Channels;
using AMQPClient.Protocol;
using Microsoft.Extensions.Logging;

namespace AMQPClient;

// FIXME: handle errors
// FIXME: stop all channels on connection close
public class ConnectionImpl : IConnection
{
    private readonly ConnectionParams _params;
    private AmqpFrameService? _amqpFrameStream;
    private CancellationTokenSource _listenersCancellationSource;
    private readonly ChannelIdGenerator _channelIdGenerator;
    private PingPongWatcher? _pingPongService;
    private readonly ILogger<ConnectionImpl> _logger = DefaultLoggerFactory.CreateLogger<ConnectionImpl>();

    private readonly ConcurrentDictionary<short, ChannelWriter<object>> _channelWriters = new();
    internal SystemChannel SystemChannel { get; set; }
    internal event Func<Task> OnConnectionClosed;

    public ConnectionImpl(ConnectionParams @params)
    {
        _params = @params;
        _channelIdGenerator = new ChannelIdGenerator();
    }
    
    public async Task StartAsync()
    {
        _amqpFrameStream = CreateStreamReader();
        _pingPongService = new PingPongWatcher(_amqpFrameStream!);
        _pingPongService.HeartbeatInterval = _params.HeartbeatInterval;
        await HandshakeAsync();
        SystemChannel = CreateSystemChannel();
        StartConnectionListeners();
    }

    private void StartConnectionListeners()
    {
        _listenersCancellationSource = new CancellationTokenSource();

        StartIncomingFramesListener(_listenersCancellationSource.Token);
        StartHeartbeatFrameListener(_listenersCancellationSource.Token);
    }

    public async Task CloseAsync()
    {
        await SystemChannel.CloseConnectionAsync();
        await _listenersCancellationSource.CancelAsync();
        await _amqpFrameStream!.DisposeAsync();
        _amqpFrameStream = null;
        await OnConnectionClosed();
    }

    private void StartHeartbeatFrameListener(CancellationToken cancellationToken = default)
    {
        Task.Run(async () => await _pingPongService!.StartAsync(cancellationToken), cancellationToken);
    }

    private AmqpFrameService CreateStreamReader()
    {
        var tcpClient = new TcpClient(_params.Host, _params.Port);
        var stream = tcpClient.GetStream();
        AmqpFrameService reader;

        if (_params.UseTls)
        {
            var sslStream = new SslStream(stream);
            sslStream.AuthenticateAsClient(_params.Host);
            reader = new AmqpFrameService(sslStream);
        }
        else
        {
            reader = new AmqpFrameService(stream);
        }

        reader.FrameSent += () => _pingPongService!.LastFrameSent = DateTimeOffset.Now.ToUnixTimeSeconds();
        reader.FrameReceived += () => _pingPongService!.LastFrameReceived = DateTimeOffset.Now.ToUnixTimeSeconds();

        return reader;
    }

    private async Task HandshakeAsync()
    {
        async Task<AmqpMethodFrame> ReadNextMethodFrame()
        {
            var nextFrame = await _amqpFrameStream!.ReadFrameAsync();

            // Skip healthcheck frame
            if (nextFrame!.Type != FrameType.Method) return (AmqpMethodFrame)(await _amqpFrameStream!.ReadFrameAsync())!;

            return (AmqpMethodFrame)nextFrame;
        }

        async Task WriteMethodFrameAsync(IFrameMethod method)
        {
            var bytes = method.Serialize();
            await _amqpFrameStream.SendFrameAsync(new AmqpFrame(0, bytes, FrameType.Method));
        }
        _logger.LogDebug("Handshake started");

        var protocolHeader = "AMQP"u8.ToArray().Concat(new byte[] { 0, 0, 9, 1 }).ToArray();
        await _amqpFrameStream!.SendRawAsync(protocolHeader);

        // Skip start method
        // TODO: Do we need to handle start method?
        await ReadNextMethodFrame();

        // FIXME: review with dynamic params
        var startOkMethod = new Protocol.Classes.Connection.StartOk()
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

        var nextFrame = await ReadNextMethodFrame();
        var tuneMethod = (Protocol.Classes.Connection.Tune)nextFrame.Method;

        // TODO: dynamic values for channelMax and frameMax
        var tuneOkMethod = new Protocol.Classes.Connection.TuneOk()
        {
            ChannelMax = tuneMethod.ChannelMax,
            Heartbeat = _params.HeartbeatInterval,
            FrameMax = tuneMethod.FrameMax
        };
        await WriteMethodFrameAsync(tuneOkMethod);

        var openMethod = new Protocol.Classes.Connection.Open()
        {
            VirtualHost = _params.Vhost
        };
        await WriteMethodFrameAsync(openMethod);
        // Skip openOk response
        await ReadNextMethodFrame();

        _logger.LogDebug("Handshake completed");
    }

    public async Task<IChannel> CreateChannelAsync()
    {
        var channel = CreateChannel();
        await channel.OpenAsync();
        return channel;
    }

    private SystemChannel CreateSystemChannel()
    {
        var trxChannel = System.Threading.Channels.Channel.CreateUnbounded<object>();
        var channel = new SystemChannel(trxChannel, _amqpFrameStream!, this);
        // Register channel
        OnConnectionClosed += channel.HandleConnectionClosed;
        _channelWriters[channel.ChannelId] = trxChannel.Writer;

        channel.StartListener();

        return channel;
    }

    private ChannelImpl CreateChannel(short? id = null)
    {
        var channelId = id ?? _channelIdGenerator.GenerateChannelId();
        var trxChannel = System.Threading.Channels.Channel.CreateUnbounded<object>();
        var channel = new ChannelImpl(trxChannel, _amqpFrameStream!, channelId);
        // Register channel
        OnConnectionClosed += channel.HandleConnectionClosed;
        _channelWriters[channelId] = trxChannel.Writer;

        return channel;
    }

    private void StartIncomingFramesListener(CancellationToken cancellationToken = default)
    {
        var listener = new IncomingFrameListener(_amqpFrameStream!, _channelWriters);
        Task.Run(async () => await listener.StartAsync(cancellationToken), cancellationToken);
    }
}