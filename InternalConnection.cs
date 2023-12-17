using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using AMQPClient.Methods;
using AMQPClient.Methods.Channels;
using AMQPClient.Methods.Connection;
using AMQPClient.Protocol;
using Decoder = AMQPClient.Protocol.Decoder;
using Encoder = AMQPClient.Protocol.Encoder;

namespace AMQPClient;

public class InternalConnection
{
    private Dictionary<int, IAmqpChannel> _channels = new();
    private AmqpStreamWrapper _amqpStreamWrapper;

    public async Task OpenAmqpConnectionAsync()
    {
        var tcpClient = new TcpClient("localhost", 5672);
        _amqpStreamWrapper = new AmqpStreamWrapper(tcpClient.GetStream());
        await HandshakeAsync();
        SpawnIncomingListener();
    }

    public async Task HandshakeAsync()
    {
        var readNextMethodFrame = async () =>
        {
            var nextFrame = await _amqpStreamWrapper.ReadFrameAsync();

            // Healthcheck frame check
            if (nextFrame.Type == FrameType.Method)
            {
                return (LowLevelAmqpMethodFrame)nextFrame;
            }

            return (LowLevelAmqpMethodFrame)await _amqpStreamWrapper.ReadFrameAsync();
        };

        var protocolHeader = Encoding.ASCII.GetBytes("AMQP").Concat(new byte[] { 0, 0, 9, 1 }).ToArray();
        await _amqpStreamWrapper.SendRawAsync(protocolHeader);

        var nextFrame = await readNextMethodFrame();
        // TODO: Do something
        var startMethod = nextFrame.castTo<StartMethod>();
        Console.WriteLine($"Received start method {startMethod}");

        // FIXME: review with dynamic params
        var startOkMethod = new StartOkMethod()
        {
            ClientProperties = new Dictionary<string, object>()
            {
                { "product", "amqp0.9.1 client" },
                { "platform", "csharp lang" },
                { "copyright", "lorem ipsum" },
                { "information", "lorem ipsum" },
            },
            Mechanism = "PLAIN",
            // Response = "\x00" + "user" + "\x00" + "password",
            Response = $"{'\x00'}user{'\x00'}password",
            Locale = "en_US",
        };
        await SendConnectionMethodAsync(startOkMethod);

        nextFrame = await readNextMethodFrame();
        var tuneMethod = nextFrame.castTo<TuneMethod>();
        Console.WriteLine($"Received TuneMethod {tuneMethod}");

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

        nextFrame = await readNextMethodFrame();
        var openOkMethod = nextFrame.castTo<OpenOkMethod>();
        Console.WriteLine($"Received open ok {openOkMethod}");
    }

    private int _channelId;
    private short NextChannelId()
    {
        Interlocked.Increment(ref _channelId);
        return (short)_channelId;
    }

    public async Task<Channel> OpenChannelAsync()
    {
        var method = new ChannelOpenMethod();
        short channelId = NextChannelId();
        var responseMethod = await SendMethodAsync<ChannelOpenOkMethod>(channelId, method);
        Console.WriteLine($"[OpenChannelAsync] Response method: {responseMethod.ToString()}");

        return new Channel(this, channelId);
    }
    
    private void SpawnIncomingListener()
    {
        Task.Run(async () =>
        {
            // FIXME: cancellation
            while (true)
            {
                var frame = await _amqpStreamWrapper.ReadFrameAsync();

                switch (frame.Type)
                {
                    case FrameType.Method:
                        var methodFrame = (LowLevelAmqpMethodFrame)frame;
                        ConcurrentQueue<TaskCompletionSource<LowLevelAmqpMethodFrame>> queue;
                        TaskCompletionSource<LowLevelAmqpMethodFrame> taskSource;

                        if (
                            AmpqMethodMap.IsAsyncResponse(methodFrame.ClassId, methodFrame.MethodId) &&
                            _methodWaitQueue.TryGetValue(methodFrame.Channel, out queue) &&
                            queue.TryDequeue(out taskSource)
                        ) {
                            taskSource.SetResult(methodFrame);
                            continue;
                        }
                        

                        throw new NotImplementedException("Not impelemented part");
                        
                        var channel = _channels.ContainsKey(frame.Channel)
                            ? _channels[frame.Channel]
                            : throw new Exception($"Invalid channel: {frame.Channel}");
                        await channel.HandleMethodFrameAsync(frame.Payload);
                        break;
                        
                    
                    default:
                        throw new Exception($"Not matched type {frame.Type}");

                }
            }
        });
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

    // FIXME: concurrent queue? or something better in general
    private Dictionary<short, ConcurrentQueue<TaskCompletionSource<LowLevelAmqpMethodFrame>>> _methodWaitQueue = new()
    {
        { 0, new() }
    };

    private async Task<TResponse> SendMethodAsync<TResponse>(short channelId, Method method) where TResponse: Method, new()
    {
        var taskSource = new TaskCompletionSource<LowLevelAmqpMethodFrame>();
        await SendMethodAsync(channelId, method);

        ConcurrentQueue<TaskCompletionSource<LowLevelAmqpMethodFrame>> sourcesQueue;

        if (!_methodWaitQueue.TryGetValue(channelId, out sourcesQueue))
        {
            sourcesQueue = new();
            _methodWaitQueue.Add(channelId, sourcesQueue);
        }
        sourcesQueue.Enqueue(taskSource);

        var frame = await taskSource.Task;

        return frame.castTo<TResponse>();
    }
}