using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading.Channels;
using AMQPClient.Protocol.Methods;
using AMQPClient.Protocol.Methods.Connection;
using AMQPClient.Protocol.Types;

namespace AMQPClient.Protocol;

// FIXME: handle errors
public class InternalConnection
{
    private const short DefaultChannelId = 0;
    private readonly Dictionary<int, IAmqpChannel> _channels = new();
    private readonly ConcurrentDictionary<short, ChannelWriter<object>> _channelWriters = new();
    private readonly ConnectionParams _params;
    private AmqpFrameStream _amqpFrameStream;
    private int _channelId;

    public InternalConnection(ConnectionParams @params)
    {
        _params = @params;
    }

    public async Task StartAsync()
    {
        var tcpClient = new TcpClient(_params.Host, _params.Port);
        _amqpFrameStream = new AmqpFrameStream(tcpClient.GetStream());
        await HandshakeAsync();
        CreateChannel(DefaultChannelId);
        StartIncomingFramesListener();
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

        Console.WriteLine("[InternalConnection]Handshake completed");
    }

    public async Task<IChannel> OpenChannelAsync()
    {
        var channel = CreateChannel();
        await channel.OpenAsync(channel.ChannelId);
        return channel;
    }

    private ChannelImpl CreateChannel(short? id = null)
    {
        var channelId = id ?? NextChannelId();
        var trxChannel = Channel.CreateUnbounded<object>();
        var channel = new ChannelImpl(trxChannel, _amqpFrameStream, this, channelId);
        _channels.Add(channelId, channel);
        _channelWriters[channelId] = trxChannel.Writer;

        return channel;
    }

    private void StartIncomingFramesListener(CancellationToken cancellationToken = default)
    {
        var listener = new IncomingFrameListener(_amqpFrameStream, _channels, _channelWriters);
        Task.Run(async () => await listener.StartAsync(cancellationToken), cancellationToken);
    }

    internal async Task SendEnvelopeAsync(short channelId, AmqpEnvelope envelope)
    {
        var properties = new HeaderProperties();
        var body = envelope.Payload!.Content;
        var methodFrame = new AmqpMethodFrame(channelId, envelope.Method);

        await _amqpFrameStream.SendFrameAsync(methodFrame);

        if (envelope.Payload == null) return;

        var headerFrame =
            new AmqpHeaderFrame(channelId, envelope.Method.ClassMethodId().Item1, body.Length, properties);
        await _amqpFrameStream.SendFrameAsync(headerFrame);

        var bodyFrame = new AmqpBodyFrame(channelId, body);
        await _amqpFrameStream.SendFrameAsync(bodyFrame);
    }

    private short NextChannelId()
    {
        Interlocked.Increment(ref _channelId);
        return (short)_channelId;
    }
}