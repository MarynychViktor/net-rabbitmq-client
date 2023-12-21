using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using AMQPClient.Methods;
using AMQPClient.Methods.Channels;
using AMQPClient.Methods.Connection;
using AMQPClient.Protocol;
using AMQPClient.Types;
using Encoder = AMQPClient.Protocol.Encoder;

namespace AMQPClient;

// FIXME: handle errors
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
    public Dictionary<string, Action<LowLevelAmqpMethodFrame>> BasicConsumers = new();

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
            Response = $"{'\x00'}user{'\x00'}password",
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
            VirtualHost = "my_vhost"
        };
        await SendMethodAsync(DefaultChannelId, openMethod);

        nextFrame = await ReadNextMethodFrame();
        var _openOkMethod = (OpenOkMethod)nextFrame.Method;

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
        var channel = new Channel(this, channelId);

        _channels.Add(channelId, channel);
        
        return channel;
    }

    private Dictionary<short, Queue<(LowLevelAmqpMethodFrame method, LowLevelAmqpHeaderFrame? header, byte[]? bodyFrame)>> pendingFrames = new();
    
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
                            var (classId, methodId) = methodFrame.Method.ClassMethodId();
                            Console.WriteLine($"Received method {classId} {methodId}");

                            if (AmpqMethodMap.HasBody(classId, methodId))
                            {
                                Queue<(LowLevelAmqpMethodFrame method, LowLevelAmqpHeaderFrame? header,
                                    byte[]? bodyFrame)> pendingChannelFrames;
                                if (!pendingFrames.TryGetValue(methodFrame.Channel, out pendingChannelFrames))
                                {
                                    pendingChannelFrames = new();
                                    pendingFrames[methodFrame.Channel] = pendingChannelFrames;
                                }

                                pendingChannelFrames.Enqueue((methodFrame, null, null));
                                continue;
                            }

                            ConcurrentQueue<TaskCompletionSource<LowLevelAmqpMethodFrame>> queue;
                            TaskCompletionSource<LowLevelAmqpMethodFrame> taskSource;

                            if (
                                AmpqMethodMap.IsAsyncResponse(classId, methodId) &&
                                _methodWaitQueue.TryGetValue(methodFrame.Channel, out queue) &&
                                queue.TryDequeue(out taskSource)
                            )
                            {
                                taskSource.SetResult(methodFrame);
                                break;
                            }
                            else
                            {
                                _channels[methodFrame.Channel].HandleFrameAsync(methodFrame);
                            }

                            throw new NotImplementedException("Not impelemented part");
                        case FrameType.ContentHeader:
                            var headerFrame = (LowLevelAmqpHeaderFrame)frame;
                            var lastMethodWoHeader = pendingFrames[headerFrame.Channel].Dequeue();
                            lastMethodWoHeader.header = headerFrame;
                            pendingFrames[headerFrame.Channel].Enqueue(lastMethodWoHeader);
                            continue;
                        case FrameType.Body:
                            var bodyFrame = (LowLevelAmqpBodyFrame)frame;
                            var lastMethodWoBody = pendingFrames[bodyFrame.Channel].Dequeue();

                            if (lastMethodWoBody.bodyFrame == null)
                            {
                                lastMethodWoBody.bodyFrame = bodyFrame.Payload;
                            }
                            else
                            {
                                lastMethodWoBody.bodyFrame = lastMethodWoBody.bodyFrame.Concat(bodyFrame.Payload).ToArray();
                            }

                            var header = lastMethodWoBody.header;
                            var bFrame = lastMethodWoBody.bodyFrame;
                            if (lastMethodWoBody.header.BodyLength > lastMethodWoBody.bodyFrame.Length)
                            {
                                pendingFrames[bodyFrame.Channel].Enqueue(lastMethodWoBody);
                                return;
                            }

                            var envelopePayload = new AmqpEnvelopePayload(
                                lastMethodWoBody.header.Properties,
                                lastMethodWoBody.bodyFrame
                            );
                            var envelope = new AmqpEnvelope(lastMethodWoBody.method.Method, envelopePayload);
                            _channels[bodyFrame.Channel].HandleEnvelopeAsync(envelope);
                            continue;
                        default:
                            throw new Exception($"Not matched type {frame.Type}");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"-------------------Listener failed with : {e}");
                throw;
            }
            finally
            {
                Console.WriteLine($"-------------------Listener exited");
            }
        });
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
        var bodyFrame = new LowLevelAmqpBodyFrame(channelId, body);

        await _amqpStreamWrapper.SendFrameAsync(headerFrame);
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
}