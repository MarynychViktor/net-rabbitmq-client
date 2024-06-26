using System.Collections.Concurrent;
using System.Threading.Channels;
using AMQPClient.Protocol;
using AMQPClient.Protocol.Methods;
using AMQPClient.Protocol.Methods.Basic;
using AMQPClient.Protocol.Methods.Channels;
using AMQPClient.Protocol.Methods.Exchanges;
using AMQPClient.Protocol.Methods.Queues;
using AMQPClient.Protocol.Types;
using Microsoft.Extensions.Logging;

namespace AMQPClient;

public class InternalChannel : ChannelBase, IChannel
{
    private readonly Dictionary<string, Action<AmqpEnvelope>> _consumersByTags = new();
    private readonly IAmqpFrameSender _frameSender;
    private readonly Channel<object> _trxChannel;

    private readonly Dictionary<short, ConcurrentQueue<TaskCompletionSource<AmqpMethodFrame>>> _synchMethodHandles =
        new();

    // Fixme: remove connection from channel
    public InternalChannel(Channel<object> trxChannel, IAmqpFrameSender frameSender, InternalConnection connection,
        short id) : base(connection, id)
    {
        _trxChannel = trxChannel;
        _frameSender = frameSender;
    }

    private ChannelReader<object> RxChannel => _trxChannel.Reader;
    private ILogger<InternalChannel> Logger { get; } = DefaultLoggerFactory.CreateLogger<InternalChannel>();

    public async Task ExchangeDeclare(string name, bool passive = false, bool durable = false, bool autoDelete = false,
        bool internalOnly = false, bool nowait = false)
    {
        var flags = ExchangeDeclareFlags.None;
        if (passive) flags &= ExchangeDeclareFlags.Passive;

        if (durable) flags &= ExchangeDeclareFlags.Durable;

        if (autoDelete) flags &= ExchangeDeclareFlags.AutoDelete;

        if (internalOnly) flags &= ExchangeDeclareFlags.Internal;

        if (nowait)
        {
            // FIXME: implement nowait
            // flags &= ExchangeDeclareFlags.NoWait;
        }

        var method = new ExchangeDeclare
        {
            Name = name,
            Flags = (byte)flags
        };

        await CallMethodAsync<ExchangeDeclareOk>(ChannelId, method);
    }

    public async Task ExchangeDelete(string name)
    {
        var method = new ExchangeDelete
        {
            Name = name
        };

        await CallMethodAsync<ExchangeDeleteOk>(ChannelId, method);
    }

    // FIXME: add actual params to method
    public async Task<string> QueueDeclare(string name = "")
    {
        var method = new QueueDeclare
        {
            Name = name
        };

        var result = await CallMethodAsync<QueueDeclareOk>(ChannelId, method);
        return result.Name;
    }

    // FIXME: add actual params to method
    public async Task QueueBind(string queue, string exchange, string routingKey)
    {
        var method = new QueueBind
        {
            Queue = queue,
            Exchange = exchange,
            RoutingKey = routingKey
        };
        await CallMethodAsync<QueueBindOk>(ChannelId, method);
    }

    // FIXME: add actual params to method
    public async Task BasicConsume(string queue, Action<AmqpEnvelope> consumer)
    {
        var method = new BasicConsume
        {
            Queue = queue
        };
        var response = await CallMethodAsync<BasicConsumeOk>(ChannelId, method);
        Console.WriteLine($"Registered consumer with tag{response.Tag}");
        _consumersByTags.Add(response.Tag, consumer);
    }

    public Task BasicPublishAsync(string exchange, string routingKey, Message message)
    {
        var method = new BasicPublish
        {
            Exchange = exchange,
            RoutingKey = routingKey
        };

        var envelopePayload = new AmqpEnvelopePayload(message.Properties, message.Data);
        var envelope = new AmqpEnvelope(method, envelopePayload);

        return _connection.SendEnvelopeAsync(ChannelId, envelope);
    }

    public async Task BasicAck(AmqpEnvelope message)
    {
        if (message.Method is BasicDeliver deliverMethod)
        {
            var method = new BasicAck
            {
                Tag = deliverMethod.DeliverTag,
                Multiple = 0
            };

            await CallMethodAsync(ChannelId, method);

            return;
        }

        throw new NotImplementedException();
    }

    internal async Task OpenAsync(short channelId)
    {
        StartListener();
        await CallMethodAsync<ChannelOpenOkMethod>(channelId, new ChannelOpenMethod());
    }

    private void StartListener(CancellationToken cancellationToken = default)
    {
        Task.Run(async () =>
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested) break;

                Logger.LogDebug("Waiting for next frame....");
                var nextFrame = await RxChannel.ReadAsync(cancellationToken);

                switch (nextFrame)
                {
                    case AmqpMethodFrame frame:
                        Logger.LogDebug("AmqpMethod frame received with type {type}", frame.Method);
                        if (frame.Method.IsAsyncResponse())
                        {
                            if (!_synchMethodHandles[ChannelId]
                                    .TryDequeue(out var result))
                                throw new Exception("No task completion source found");

                            result.SetResult(frame);
                        }

                        if (frame.Method.HasBody())
                        {
                            // TODO: remove envolope class
                            var envelopePayload = new AmqpEnvelopePayload(
                                frame.Properties,
                                frame.Body
                            );

                            var envelope = new AmqpEnvelope(frame.Method, envelopePayload);
                            if (envelope.Method is BasicDeliver method)
                                _consumersByTags[method.ConsumerTag].Invoke(envelope);
                        }

                        break;
                    default:
                        throw new Exception("Unknown frame");
                }
            }

            return Task.CompletedTask;
        }, cancellationToken);
    }

    private async Task<TResponse> CallMethodAsync<TResponse>(short channelId, Method method)
        where TResponse : Method, new()
    {
        var taskSource = new TaskCompletionSource<AmqpMethodFrame>(TaskCreationOptions.RunContinuationsAsynchronously);
        await CallMethodAsync(channelId, method);

        if (!_synchMethodHandles.TryGetValue(channelId, out var sourcesQueue))
        {
            sourcesQueue = new ConcurrentQueue<TaskCompletionSource<AmqpMethodFrame>>();
            _synchMethodHandles[channelId] = sourcesQueue;
        }

        sourcesQueue.Enqueue(taskSource);

        return (TResponse)(await taskSource.Task).Method;
    }

    private Task CallMethodAsync(short channel, Method method)
    {
        var bytes = Encoder.MarshalMethodFrame(method);
        return _frameSender.SendFrameAsync(new AmqpFrame(channel, bytes, FrameType.Method));
    }
}