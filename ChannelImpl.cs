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

// Fixme: remove connection from channel
public class ChannelImpl(
    Channel<object> trxChannel,
    IAmqpFrameSender frameSender,
    short id)
    : ChannelBase(frameSender, id), IChannel
{
    private readonly Dictionary<string, Action<AmqpEnvelope>> _consumersByTags = new();
    private ChannelReader<object> RxChannel => trxChannel.Reader;
    private ILogger<ChannelImpl> Logger { get; } = DefaultLoggerFactory.CreateLogger<ChannelImpl>();
    private CancellationTokenSource _listenerCancellationSource;

    public async Task ExchangeDeclare(string name, bool passive = false, bool durable = false, bool autoDelete = false,
        bool internalOnly = false, bool nowait = false)
    {
        var flags = ExchangeDeclareFlags.None;
        if (passive) flags |= ExchangeDeclareFlags.Passive;
        if (durable) flags |= ExchangeDeclareFlags.Durable;
        if (autoDelete) flags |= ExchangeDeclareFlags.AutoDelete;
        if (internalOnly) flags |= ExchangeDeclareFlags.Internal;
        if (nowait) flags |= ExchangeDeclareFlags.NoWait;

        var method = new ExchangeDeclare
        {
            Name = name,
            Flags = (byte)flags
        };

        if (nowait)
        {
            await CallMethodAsync(ChannelId, method);
            return;
        }

        await CallMethodAsync<ExchangeDeclareOk>(ChannelId, method);
    }

    public async Task Flow(bool active)
    {
        var method = new ChannelFlowMethod()
        {
            Active = (byte)(active ? 1 : 0)
        };
        await CallMethodAsync<ChannelFlowOkMethod>(ChannelId, method);
    }

    public async Task Close()
    {
        var method = new ChannelCloseMethod();
        IsClosed = true;
        await CallMethodAsync<ChannelCloseOkMethod>(ChannelId, method, checkForClosed: false);
        await _listenerCancellationSource.CancelAsync();
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

    public async Task BasicPublishAsync(string exchange, string routingKey, Message message)
    {
        var method = new BasicPublish
        {
            Exchange = exchange,
            RoutingKey = routingKey
        };

        await CallMethodAsync(ChannelId, method, message.Properties, message.Data);
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
        _listenerCancellationSource = new CancellationTokenSource();
        StartListener(_listenerCancellationSource.Token);
        await CallMethodAsync<ChannelOpenOkMethod>(channelId, new ChannelOpenMethod());
    }

    private void StartListener(CancellationToken cancellationToken = default)
    {
        Task.Run(async () =>
        {
            try
            {
                while (true)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        Logger.LogInformation("Stopping listener loop...");
                        break;
                    }

                    Logger.LogDebug("Waiting for next frame....");
                    var nextFrame = await RxChannel.ReadAsync(cancellationToken);

                    switch (nextFrame)
                    {
                        case AmqpMethodFrame frame:
                            Logger.LogDebug("AmqpMethod frame received with type {type}", frame.Method);
                            if (frame.Method.IsAsyncResponse())
                            {
                                if (!SyncMethodHandles[ChannelId]
                                        .TryDequeue(out var result))
                                    throw new Exception("No task completion source found");

                                result.SetResult(frame);
                                break;
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
                                {
                                    _consumersByTags[method.ConsumerTag].Invoke(envelope);
                                    break;
                                }
                            }

                            throw new NotImplementedException();
                        default:
                            throw new Exception("Unknown frame");
                    }
                }
            } catch (OperationCanceledException e) {
                Logger.LogWarning("Frame loop exited");
            } catch (Exception e)
            {
                Logger.LogCritical("StartListener loop failed with {msg}", e.Message);
                throw;
            }
            return Task.CompletedTask;
        }, cancellationToken);
    }
}