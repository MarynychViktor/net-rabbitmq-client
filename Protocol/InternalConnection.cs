using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using AMQPClient.Protocol.Methods;
using AMQPClient.Protocol.Methods.Channels;
using AMQPClient.Protocol.Methods.Connection;
using AMQPClient.Protocol.Types;

namespace AMQPClient.Protocol;

// FIXME: handle errors
public class InternalConnection
{
    private readonly ConnectionParams _params;
    private const short DefaultChannelId = 0;
    private const string Product = "Amqp 0.9.1 client";
    private const string Platform = ".Net Core";
    private const string Copyright = "Lorem ipsum";
    private const string Information = "Lorem ipsum";
    private int _channelId;
    private Dictionary<int, IAmqpChannel> _channels = new();
    private AmqpStreamWrapper _amqpStreamWrapper;
    public Dictionary<string, Action<LowLevelAmqpMethodFrame>> BasicConsumers = new();

    public InternalConnection(ConnectionParams @params)
    {
        _params = @params;
    }

    // FIXME: concurrent queue? or something better in general
    private Dictionary<short, ConcurrentQueue<TaskCompletionSource<LowLevelAmqpMethodFrame>>> _methodWaitQueue = new()
    {
        { DefaultChannelId, new() }
    };

    public async Task StartAsync()
    {
        var tcpClient = new TcpClient(_params.Host, _params.Port);
        _amqpStreamWrapper = new AmqpStreamWrapper(tcpClient.GetStream());
        await HandshakeAsync();
        SpawnIncomingListener();
    }

    public async Task HandshakeAsync()
    {
        async Task<LowLevelAmqpMethodFrame> ReadNextMethodFrame()
        {
            var nextFrame = await _amqpStreamWrapper.ReadFrameAsync();

            // Healthcheck frame check
            if (nextFrame.Type == FrameType.Method)
            {
                return (LowLevelAmqpMethodFrame)nextFrame;
            }

            return (LowLevelAmqpMethodFrame)await _amqpStreamWrapper.ReadFrameAsync();
        }

        var protocolHeader = Encoding.ASCII.GetBytes("AMQP").Concat(new byte[] { 0, 0, 9, 1 }).ToArray();
        await _amqpStreamWrapper.SendRawAsync(protocolHeader);

        var nextFrame = await ReadNextMethodFrame();
        // TODO: Do something
        var _startMethod = (StartMethod)nextFrame.Method;

        // FIXME: review with dynamic params
        var startOkMethod = new StartOkMethod()
        {
            ClientProperties = new Dictionary<string, object>()
            {
                { "product", Product },
                { "platform", Platform },
                { "copyright", Copyright },
                { "information", Information },
            },
            Mechanism = "PLAIN",
            // Response = "\x00" + "user" + "\x00" + "password",
            Response = $"{'\x00'}{_params.User}{'\x00'}{_params.Password}",
            Locale = "en_US",
        };
        await SendMethodAsync(DefaultChannelId, startOkMethod);

        nextFrame = await ReadNextMethodFrame();
        var tuneMethod = (TuneMethod)nextFrame.Method;

        var tuneOkMethod = new TuneOkMethod()
        {
            ChannelMax = tuneMethod.ChannelMax,
            Heartbeat = tuneMethod.Heartbeat,
            FrameMax = tuneMethod.FrameMax,
        };
        await SendMethodAsync(DefaultChannelId, tuneOkMethod);

        var openMethod = new OpenMethod()
        {
            VirtualHost = _params.Vhost
        };
        await SendMethodAsync(DefaultChannelId, openMethod);

        nextFrame = await ReadNextMethodFrame();
        var _openOkMethod = (OpenOkMethod)nextFrame.Method;

        Console.WriteLine("[InternalConnection]Handshake completed");
    }

    public async Task<InternalChannel> OpenChannelAsync()
    {
        var method = new ChannelOpenMethod();
        short channelId = NextChannelId();
        await SendMethodAsync<ChannelOpenOkMethod>(channelId, method);
        var channel = new InternalChannel(this, channelId);

        _channels.Add(channelId, channel);
        
        return channel;
    }

    private Dictionary<short, Queue<(LowLevelAmqpMethodFrame method, LowLevelAmqpHeaderFrame? header, byte[]? bodyFrame)>> pendingFrames = new();
    
    private void SpawnIncomingListener()
    {
        var listener = new IncomingFrameListener(_amqpStreamWrapper, _channels, _methodWaitQueue);
        Task.Run(async () => await listener.StartAsync());
    }


    internal async Task SendEnvelopeAsync(short channelId, AmqpEnvelope envelope)
    {
        var properties = new HeaderProperties();
        var body = envelope.Payload!.Content;
        var methodFrame = new LowLevelAmqpMethodFrame(channelId, envelope.Method);

        await _amqpStreamWrapper.SendFrameAsync(methodFrame);

        if (envelope.Payload == null)
        {
            return;
        }

        var headerFrame = new LowLevelAmqpHeaderFrame(channelId, envelope.Method.ClassMethodId().Item1, body.Length, properties);
        await _amqpStreamWrapper.SendFrameAsync(headerFrame);

        var bodyFrame = new LowLevelAmqpBodyFrame(channelId, body);
        await _amqpStreamWrapper.SendFrameAsync(bodyFrame);
    }

    internal Task SendMethodAsync(short channel, Method method)
    {
        // FIXME: if method has body, write it should send multiple raw frames
        var bytes = Encoder.MarshalMethodFrame(method);
        return _amqpStreamWrapper.SendFrameAsync(new LowLevelAmqpFrame(channel, bytes, FrameType.Method));
    }

    internal async Task<TResponse> SendMethodAsync<TResponse>(short channelId, Method method) where TResponse: Method, new()
    {
        var taskSource = new TaskCompletionSource<LowLevelAmqpMethodFrame>(TaskCreationOptions.RunContinuationsAsynchronously);
        await SendMethodAsync(channelId, method);
        ConcurrentQueue<TaskCompletionSource<LowLevelAmqpMethodFrame>> sourcesQueue;

        if (!_methodWaitQueue.TryGetValue(channelId, out sourcesQueue))
        {
            sourcesQueue = new();
            _methodWaitQueue.Add(channelId, sourcesQueue);
        }

        sourcesQueue.Enqueue(taskSource);
        var frame = await taskSource.Task;

        return (TResponse)frame.Method;
    }
    
    private short NextChannelId()
    {
        Interlocked.Increment(ref _channelId);
        return (short)_channelId;
    }
}