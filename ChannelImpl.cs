using System.Threading.Channels;
using AMQPClient.Protocol;
using AMQPClient.Protocol.Methods.Basic;
using AMQPClient.Protocol.Methods.Channels;
using AMQPClient.Protocol.Methods.Exchanges;
using AMQPClient.Protocol.Methods.Queues;
using Microsoft.Extensions.Logging;

namespace AMQPClient;

internal class ChannelImpl(Channel<object> trxChannel, IAmqpFrameSender frameSender, short id)
    : ChannelBase(frameSender, id), IChannel
{
    private readonly Dictionary<string, Action<IMessage>> _consumersByTags = new();
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
            await CallMethodAsync(method);
            return;
        }

        await CallMethodAsync<ExchangeDeclareOk>(method);
    }

    public async Task Flow(bool active)
    {
        var method = new ChannelFlowMethod()
        {
            Active = (byte)(active ? 1 : 0)
        };

        await CallMethodAsync<ChannelFlowOkMethod>(method);
    }

    public async Task Close()
    {
        var method = new ChannelCloseMethod();
        State = ChannelState.Closed;
        await CallMethodAsync<ChannelCloseOkMethod>(method, checkForClosed: false);
        await _listenerCancellationSource.CancelAsync();
    }

    public async Task ExchangeDelete(string name)
    {
        var method = new ExchangeDelete
        {
            Name = name
        };

        await CallMethodAsync<ExchangeDeleteOk>(method);
    }

    public async Task<string> QueueDeclare(string name = "")
    {
        var method = new QueueDeclare
        {
            Name = name
        };

        var result = await CallMethodAsync<QueueDeclareOk>(method);
        return result.Name;
    }

    public async Task QueueBind(string queue, string exchange, string routingKey)
    {
        var method = new QueueBind
        {
            Queue = queue,
            Exchange = exchange,
            RoutingKey = routingKey
        };
        await CallMethodAsync<QueueBindOk>(method);
    }

    public async Task QueueUnbind(string queue, string exchange, string routingKey, Dictionary<string, object>? arguments = null)
    {
        var method = new QueueUnbind()
        {
            Queue = queue,
            Exchange = exchange,
            RoutingKey = routingKey,
            Arguments = arguments ?? new(),
        };
        await CallMethodAsync<QueueUnbindOk>(method);
    }

    public async Task QueuePurge(string queue, bool noWait = false)
    {
        var method = new QueuePurge()
        {
            Queue = queue,
            NoWait = (byte)(noWait ? 1 : 0)
        };

        await CallMethodAsync<QueuePurgeOk>(method);
    }

    // TODO: add precondition errors handling
    public async Task QueueDelete(string queue, bool ifUnused = false, bool ifEmpty = false, bool noWait = false)
    {
        var flags = QueueDeleteFlags.None;
        if (ifUnused) flags |= QueueDeleteFlags.IfUnused;
        if (ifEmpty) flags |= QueueDeleteFlags.IfEmpty;
        if (noWait) flags |= QueueDeleteFlags.NoWait;

        var method = new QueueDelete()
        {
            Queue = queue,
            Flags = (byte)flags,
        };

        if (noWait)
        {
            await CallMethodAsync(method);
        }
        else
        {
            await CallMethodAsync<QueueDeleteOk>(method);
        }
    }

    public async Task<string> BasicConsume(string queue, Action<IMessage> consumer)
    {
        var method = new BasicConsume
        {
            Queue = queue
        };

        var response = await CallMethodAsync<BasicConsumeOk>(method);
        Logger.LogDebug("Registered consumer with tag: {tag}", response.Tag);

        _consumersByTags.Add(response.Tag, consumer);
        return response.Tag;
    }

    public async Task BasicCancel(string consumerTag, bool noWait = false)
    {
        var method = new BasicCancel()
        {
            ConsumerTag = consumerTag,
            NoWait = (byte)(noWait ? 1 : 0)
        };

        if (noWait)
        {
            await CallMethodAsync(method);
            _consumersByTags.Remove(consumerTag);
        }
        else
        {
            var response = await CallMethodAsync<BasicCancelOk>(method);
            _consumersByTags.Remove(response.ConsumerTag);
        }
    }

    public async Task BasicPublishAsync(string exchange, string routingKey, IMessage message)
    {
        var method = new BasicPublish
        {
            Exchange = exchange,
            RoutingKey = routingKey
        };

        await CallMethodAsync(method, message.Properties ?? new HeaderProperties(), message.Content);
    }

    public async Task BasicAck(IMessage message)
    {
        if (message.DeliveryTag is {} deliveryTag)
        {
            var method = new BasicAck
            {
                Tag = deliveryTag,
                // TODO: add support for this param
                Multiple = 0
            };

            await CallMethodAsync(method);
            return;
        }

        // TODO: check if reachable
        throw new NotImplementedException();
    }

    public async Task BasicReject(IMessage message, bool requeue = false)
    {
        if (message.DeliveryTag is {} deliveryTag)
        {
            var method = new BasicReject()
            {
                Tag = deliveryTag,
                Requeue = (byte)(requeue ? 1 : 0)
            };

            await CallMethodAsync(method);
            return;
        }

        // TODO: check if reachable
        throw new NotImplementedException();
    }

    public async Task BasicRecover()
    {
        // Recovery with requeue=false is not supported.
        // https://www.rabbitmq.com/docs/specification
        var method = new BasicRecover()
        {
            Requeue = 1
        };
        await CallMethodAsync<BasicRecoverOk>(method);
    }

    public async Task BasicQos(short prefetchCount, bool global = false)
    {
        var method = new BasicQos()
        {
            // Not implemented in rabbitmq
            // https://www.rabbitmq.com/amqp-0-9-1-reference#domain.short
            PrefetchSize = 0,
            PrefetchCount = prefetchCount,
            Global = (byte)(global ? 1 : 0)
        };

        await CallMethodAsync<BasicQosOk>(method);
    }

    public async Task<IMessage?> BasicGet(string queue, bool noAck = false)
    {
        var method = new BasicGet()
        {
            QueueName = queue,
            NoAck = (byte)(noAck ? 1 : 0)
        };

        var result = await CallFrameAsync(method);
        if (result.Method is BasicGetOk getOkMethod)
        {
            var frame = result.MethodFrame;
            return new IncomingMessage(frame.Body, frame.Properties, getOkMethod.DeliverTag,
                getOkMethod.Redelivered == 1, getOkMethod.Exchange, getOkMethod.RoutingKey);
        }

        if (result.Method is BasicGetEmpty) return null;

        throw new Exception($"Unexpected result {result}");
    }

    internal async Task OpenAsync()
    {
        _listenerCancellationSource = new CancellationTokenSource();
        StartListener(_listenerCancellationSource.Token);
        await CallMethodAsync<ChannelOpenOkMethod>(new ChannelOpenMethod());
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
                            Logger.LogDebug("Frame received:\n\t {method}", frame.Method);
                            if (TryHandleAsyncResponse(frame)) break;

                            await HandleIncomingMessage(frame);
                            break;
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

    private bool TryHandleAsyncResponse(AmqpMethodFrame frame)
    {
        if (!frame.Method.IsAsyncResponse()) return false;

        if (!SyncMethodHandles.TryDequeue(out var result))
            throw new Exception("No task completion source found");

        result.SetResult(new MethodResult(frame));
        return true;
    }

    private async Task HandleIncomingMessage(AmqpMethodFrame frame)
    {
        if (TryHandleConsumerMessage(frame)) return;

        switch (frame.Method)
        {
            case ChannelCloseMethod closeMethod:
                Logger.LogError("Closing channel\n {code}, {text} ", closeMethod.ReplyCode, closeMethod.ReplyText);

                var result = new MethodResult(null, closeMethod.ReplyCode, closeMethod.ReplyText);

                if (SyncMethodHandles.TryDequeue(out var source))
                {
                    source.SetResult(result);
                }

                await CallMethodAsync(new ChannelCloseOkMethod());
                LastErrorResult = result;
                State = ChannelState.Failed;
                break;
            default:
                throw new NotImplementedException();
        }
    }
    
    private bool TryHandleConsumerMessage(AmqpMethodFrame frame)
    {
        if (!frame.Method.HasBody() || frame.Method is not BasicDeliver method) return false;

        var message2 = new IncomingMessage(frame.Body, frame.Properties, method.DeliverTag, method.Redelivered == 1, method.Exchange, method.RoutingKey);
        _consumersByTags[method.ConsumerTag].Invoke(message2);
        return true;
    }
}