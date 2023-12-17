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
    private const short DefaultChannelId = 0;
    private const string Product = "Amqp 0.9.1 client";
    private const string Platform = ".Net Core";
    private const string Copyright = "Lorem ipsum";
    private const string Information = "Lorem ipsum";
    private int _channelId;
    private Dictionary<int, IAmqpChannel> _channels = new();
    private AmqpStreamWrapper _amqpStreamWrapper;

    // FIXME: concurrent queue? or something better in general
    private Dictionary<short, ConcurrentQueue<TaskCompletionSource<LowLevelAmqpMethodFrame>>> _methodWaitQueue = new()
    {
        { DefaultChannelId, new() }
    };

    public async Task OpenAmqpConnectionAsync()
    {
        var tcpClient = new TcpClient("localhost", 5672);
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
        nextFrame.castTo<StartMethod>();

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
            Response = $"{'\x00'}user{'\x00'}password",
            Locale = "en_US",
        };
        await SendMethodAsync(DefaultChannelId, startOkMethod);

        nextFrame = await ReadNextMethodFrame();
        var tuneMethod = nextFrame.castTo<TuneMethod>();

        var tuneOkMethod = new TuneOkMethod()
        {
            ChannelMax = tuneMethod.ChannelMax,
            Heartbeat = tuneMethod.Heartbeat,
            FrameMax = tuneMethod.FrameMax,
        };
        await SendMethodAsync(DefaultChannelId, tuneOkMethod);

        var openMethod = new OpenMethod()
        {
            VirtualHost = "my_vhost"
        };
        await SendMethodAsync(DefaultChannelId, openMethod);

        nextFrame = await ReadNextMethodFrame();
        nextFrame.castTo<OpenOkMethod>();

        Console.WriteLine("[InternalConnection]Handshake completed");
    }

    private short NextChannelId()
    {
        Interlocked.Increment(ref _channelId);
        return (short)_channelId;
    }

    public async Task<Channel> OpenChannelAsync()
    {
        var method = new ChannelOpenMethod();
        short channelId = NextChannelId();
        await SendMethodAsync<ChannelOpenOkMethod>(channelId, method);

        return new Channel(this, channelId);
    }
    
    private void SpawnIncomingListener()
    {
        Task.Run(async () =>
        {
            try
            {

                // FIXME: cancellation
                while (true)
                {
                    var frame = await _amqpStreamWrapper.ReadFrameAsync();

                    switch (frame.Type)
                    {
                        case FrameType.Method:
                            var methodFrame = (LowLevelAmqpMethodFrame)frame;
                            Console.WriteLine($"Received method {methodFrame.ClassId} {methodFrame.MethodId}");
                            ConcurrentQueue<TaskCompletionSource<LowLevelAmqpMethodFrame>> queue;
                            TaskCompletionSource<LowLevelAmqpMethodFrame> taskSource;

                            if (
                                AmpqMethodMap.IsAsyncResponse(methodFrame.ClassId, methodFrame.MethodId) &&
                                _methodWaitQueue.TryGetValue(methodFrame.Channel, out queue) &&
                                queue.TryDequeue(out taskSource)
                            )
                            {
                                taskSource.SetResult(methodFrame);
                                continue;
                            }
                            else
                            {
                                _channels[methodFrame.Channel].HandleFrameAsync(methodFrame);
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
            }
            catch (Exception e)
            {
                Console.WriteLine($"Listener failed with : {e}");
                throw;
            }
        });
    }

    internal Task SendMethodAsync(short channel, Method method)
    {
        // FIXME: if method has body, write it should send multiple raw frames
        var bytes = Encoder.MarshalMethodFrame(method);
        return _amqpStreamWrapper.SendFrameAsync(new LowLevelAmqpFrame(channel, bytes, FrameType.Method));
    }

    internal async Task<TResponse> SendMethodAsync<TResponse>(short channelId, Method method) where TResponse: Method, new()
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