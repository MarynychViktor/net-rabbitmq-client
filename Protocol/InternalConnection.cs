using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using System.Threading.Channels;
using AMQPClient.Protocol.Methods;
using AMQPClient.Protocol.Methods.Channels;
using AMQPClient.Protocol.Methods.Connection;
using AMQPClient.Protocol.Methods.Exchanges;
using AMQPClient.Protocol.Methods.Queues;
using AMQPClient.Protocol.Types;

namespace AMQPClient.Protocol;

// FIXME: handle errors
public class InternalConnection : IChannelCommandExecutor
{
    private readonly ConnectionParams _params;
    private const short DefaultChannelId = 0;
    private const string Product = "Amqp 0.9.1 client";
    private const string Platform = ".Net Core";
    private const string Copyright = "Lorem ipsum";
    private const string Information = "Lorem ipsum";
    private int _channelId;
    private readonly Dictionary<int, IAmqpChannel> _channels = new();
    private readonly ConcurrentDictionary<short, ChannelWriter<object>> _channelWriters = new();
    private AmqpFrameStream _amqpFrameStream;
    private MethodCaller _methodCaller;

    public InternalConnection(ConnectionParams @params)
    {
        _params = @params;
    }

    // FIXME: concurrent queue? or something better in general
    private Dictionary<short, ConcurrentQueue<TaskCompletionSource<AmqpMethodFrame>>> _methodWaitQueue = new()
    {
        { DefaultChannelId, new() }
    };

    public async Task StartAsync()
    {
        var tcpClient = new TcpClient(_params.Host, _params.Port);
        _amqpFrameStream = new AmqpFrameStream(tcpClient.GetStream());
        _methodCaller = new MethodCaller(_amqpFrameStream, _methodWaitQueue);
        await HandshakeAsync();
        SpawnIncomingListener();
    }

    public async Task HandshakeAsync()
    {
        async Task<AmqpMethodFrame> ReadNextMethodFrame()
        {
            var nextFrame = await _amqpFrameStream.ReadFrameAsync();

            // Healthcheck frame check
            if (nextFrame.Type == FrameType.Method)
            {
                return (AmqpMethodFrame)nextFrame;
            }

            return (AmqpMethodFrame)await _amqpFrameStream.ReadFrameAsync();
        }

        async Task WriteMethodFrameAsync(Method method)
        {
            var bytes = Encoder.MarshalMethodFrame(method);
            await _amqpFrameStream.SendFrameAsync(new AmqpFrame(DefaultChannelId, bytes, FrameType.Method));
        }

        var protocolHeader = "AMQP"u8.ToArray().Concat(new byte[] { 0, 0, 9, 1 }).ToArray();
        await _amqpFrameStream.SendRawAsync(protocolHeader);

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
        await WriteMethodFrameAsync(startOkMethod);

        nextFrame = await ReadNextMethodFrame();
        var tuneMethod = (TuneMethod)nextFrame.Method;

        var tuneOkMethod = new TuneOkMethod()
        {
            ChannelMax = tuneMethod.ChannelMax,
            Heartbeat = tuneMethod.Heartbeat,
            FrameMax = tuneMethod.FrameMax,
        };
        await WriteMethodFrameAsync(tuneOkMethod);

        var openMethod = new OpenMethod()
        {
            VirtualHost = _params.Vhost
        };
        await WriteMethodFrameAsync(openMethod);

        nextFrame = await ReadNextMethodFrame();
        // TODO: handle response?
        var _openOkMethod = (OpenOkMethod)nextFrame.Method;

        Console.WriteLine("[InternalConnection]Handshake completed");
    }

    public async Task<InternalChannel> OpenChannelAsync()
    {
        var channelId = NextChannelId();
        var ch = Channel.CreateUnbounded<object>();
        var channel = new InternalChannel(ch, _methodCaller,this, channelId);
        _channels.Add(channelId, channel);
        var reader = ch.Reader;
        
        _channelWriters[channelId] = ch.Writer;
        // Task.Run(async () =>
        // {
        //     while (true)
        //     {
        //         var res = await reader.ReadAsync();
        //         Console.WriteLine($"Readed for channel {channelId} - {res}");
        //     }
        // });
        await channel.OpenAsync(channelId);

        return channel;
    }

    private void SpawnIncomingListener()
    {
        var listener = new IncomingFrameListener(_amqpFrameStream, _channels, _methodWaitQueue, _channelWriters);
        Task.Run(async () => await listener.StartAsync());
    }

    internal async Task SendEnvelopeAsync(short channelId, AmqpEnvelope envelope)
    {
        var properties = new HeaderProperties();
        var body = envelope.Payload!.Content;
        var methodFrame = new AmqpMethodFrame(channelId, envelope.Method);

        await _amqpFrameStream.SendFrameAsync(methodFrame);

        if (envelope.Payload == null)
        {
            return;
        }

        var headerFrame = new AmqpHeaderFrame(channelId, envelope.Method.ClassMethodId().Item1, body.Length, properties);
        await _amqpFrameStream.SendFrameAsync(headerFrame);

        var bodyFrame = new AmqpBodyFrame(channelId, body);
        await _amqpFrameStream.SendFrameAsync(bodyFrame);
    }

    public void RegisterChannel(short channelId, IAmqpChannel channel)
    {
        _channels.Add(channelId, channel);
    }
    
    public short NextChannelId()
    {
        Interlocked.Increment(ref _channelId);
        return (short)_channelId;
    }
}