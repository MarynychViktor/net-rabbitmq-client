using System.Net.Sockets;
using System.Text;
using AMQPClient.Methods;
using AMQPClient.Methods.Connection;
using AMQPClient.Protocol;
using Encoder = AMQPClient.Protocol.Encoder;

namespace AMQPClient;

public class InternalConnection
{
    private Dictionary<int, IAmqpChannel> _channels = new();
    private AmqpStreamWrapper _amqpStreamWrapper;

    // private int _channelId;
    // private BlockingCollection<object> queue = new ();
    // private DefaultAmqpChannel _defaultChannel => (DefaultAmqpChannel)_channels[0];

    public async Task OpenAmqpConnectionAsync()
    {
        var tcpClient = new TcpClient("localhost", 5672);
        _amqpStreamWrapper = new AmqpStreamWrapper(tcpClient.GetStream());
        await HandshakeAsync();
    }

    public async Task HandshakeAsync()
    {
        var protocolHeader = Encoding.ASCII.GetBytes("AMQP").Concat(new byte[] { 0, 0, 9, 1 }).ToArray();
        await _amqpStreamWrapper.SendRawAsync(protocolHeader);

        var ReadNextMethodFrame = async () =>
        {
            var nextFrame = await _amqpStreamWrapper.ReadFrameAsync();

            // Healthcheck frame check
            if (nextFrame.Type == FrameType.Method)
            {
                return (LowLevelAmqpMethodFrame)nextFrame;
            }

            return (LowLevelAmqpMethodFrame)await _amqpStreamWrapper.ReadFrameAsync();
        };

        var nextFrame = await ReadNextMethodFrame();
        // TODO: Do something
        var startMethod = nextFrame.castTo<StartMethod>();

        // FIXME: review with dynamic params
        var startOkMethod = new StartOkMethod()
        {
            ClientProperties = new Dictionary<string, object>()
            {
                { "product", "AMQP client" },
                { "platform", ".NetCore 8.0" },
                { "copyright", "Foo" },
                { "information", "Bar" },
            },
            Mechanism = "PLAIN",
            Response = "\x00" + "user" + "\x00" + "password",
            Locale = "en_US",
        };

        await SendConnectionMethodAsync(startOkMethod);

        nextFrame = await ReadNextMethodFrame();
        var tuneMethod = nextFrame.castTo<TuneMethod>();
        var tuneOkMethod = new TuneOkMethod()
        {
            ChannelMax = tuneMethod.ChannelMax,
            Heartbeat = tuneMethod.Heartbeat,
            FrameMax = tuneMethod.FrameMax,
        };
        await SendConnectionMethodAsync(tuneOkMethod);

        var openMethod = new OpenMethod()
        {
            VirtualHost = "my_vhost"
        };
        await SendConnectionMethodAsync(openMethod);

        nextFrame = await ReadNextMethodFrame();
        var openOkMethod = nextFrame.castTo<OpenOkMethod>();
    }

    private Task SendConnectionMethodAsync(Method method)
    {
        return SendMethodAsync(0, method);
    }

    private async Task SendMethodAsync(short channel, Method method)
    {
        // FIXME: if method has body, write it should send multiple raw frames
        var bytes = Encoder.MarshalMethodFrame(method);
        await _amqpStreamWrapper.SendFrameAsync(new LowLevelAmqpFrame(channel, bytes, FrameType.Method));
    }

    private Dictionary<short, Dictionary<Type, List<TaskCompletionSource<Method>>>> _methodWaitQueue = new()
    {
        { 0, new() }
    };
    
    private async Task<TResponse> SendMethodAsync<TResponse>(Method method) where TResponse: Method
    {
        var taskSource = new TaskCompletionSource<Method>();
        await SendConnectionMethodAsync(method);

        List<TaskCompletionSource<Method>> typeSources;
        _methodWaitQueue[0].TryGetValue(typeof(TResponse), out typeSources);

        if (typeSources == null)
        {
            typeSources = new List<TaskCompletionSource<Method>>();
        }

        typeSources.Add(taskSource);

        return (TResponse) await taskSource.Task;
    }
}