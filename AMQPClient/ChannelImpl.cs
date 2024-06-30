using System.Threading.Channels;
using AMQPClient.Protocol;
using AMQPClient.Protocol.Classes;
using Microsoft.Extensions.Logging;
using Channel = AMQPClient.Protocol.Classes.Channel;

namespace AMQPClient;

internal class ChannelImpl(Channel<object> trxChannel, IAmqpFrameSender frameSender, short id)
    : ChannelBase(frameSender, id), IChannel
{
    private readonly Dictionary<string, Action<IMessage>> _consumersByTags = new();
    private ChannelReader<object> RxChannel => trxChannel.Reader;
    private ILogger<ChannelImpl> Logger { get; } = DefaultLoggerFactory.CreateLogger<ChannelImpl>();
    private CancellationTokenSource _listenerCancellationSource;

    public async Task ExchangeDeclare(string name, bool passive = false, bool durable = false, bool autoDelete = false,
        bool internalOnly = false, bool nowait = false, string type = "direct")
    {
        var method = new Exchange.Declare()
        {
            Exchange = name,
            Type = type,
            Passive = passive,
            Durable = durable,
            Reserved2 = autoDelete,
            Reserved3 = internalOnly,
            NoWait = nowait,
            Arguments = new(),
        };

        if (nowait)
        {
            await CallMethodAsync(method);
            return;
        }

        await CallMethodAsync<Exchange.DeclareOk>(method);
    }

    public async Task Flow(bool active)
    {
        var method = new Channel.Flow()
        {
            Active = active
        };

        await CallMethodAsync<Channel.FlowOk>(method);
    }

    public async Task Close()
    {
        var method = new Channel.Close();
        State = ChannelState.Closed;
        await CallMethodAsync<Channel.CloseOk>(method, checkForClosed: false);
        await _listenerCancellationSource.CancelAsync();
    }

    public async Task ExchangeDelete(string name)
    {
        var method = new Exchange.Delete()
        {
            Exchange = name
        };

        await CallMethodAsync<Exchange.DeleteOk>(method);
    }

    // TODO: declare args
    public async Task<string> QueueDeclare(string name = "")
    {
        var args = new Dictionary<string, object>();
        var method = new Queue.Declare()
        {
            Queue = name,
            Arguments = args,
            Durable = true,
        };

        var result = await CallMethodAsync<Queue.DeclareOk>(method);
        return result.Queue;
    }

    public async Task QueueBind(string queue, string exchange, string routingKey)
    {
        var method = new Queue.Bind()
        {
            Queue = queue,
            Exchange = exchange,
            RoutingKey = routingKey,
            Arguments = new()
        };
        await CallMethodAsync<Queue.BindOk>(method);
    }

    public async Task QueueUnbind(string queue, string exchange, string routingKey, Dictionary<string, object>? arguments = null)
    {
        var method = new Queue.Unbind()
        {
            Queue = queue,
            Exchange = exchange,
            RoutingKey = routingKey,
            Arguments = arguments ?? new(),
        };
        await CallMethodAsync<Queue.UnbindOk>(method);
    }

    public async Task QueuePurge(string queue, bool noWait = false)
    {
        var method = new Queue.Purge()
        {
            Queue = queue,
            NoWait = noWait
        };

        await CallMethodAsync<Queue.PurgeOk>(method);
    }

    // TODO: add precondition errors handling
    public async Task QueueDelete(string queue, bool ifUnused = false, bool ifEmpty = false, bool noWait = false)
    {
        var method = new Queue.Delete()
        {
            Queue = queue,
            IfUnused = ifUnused,
            IfEmpty = ifEmpty,
            NoWait = noWait
        };

        if (noWait)
        {
            await CallMethodAsync(method);
        }
        else
        {
            await CallMethodAsync<Queue.DeleteOk>(method);
        }
    }

    public async Task<string> BasicConsume(string queue, Action<IMessage> consumer)
    {
        var method = new Basic.Consume()
        {
            Queue = queue,
            ConsumerTag = "",
            Arguments = new(),
        };

        var response = await CallMethodAsync<Basic.ConsumeOk>(method);
        Logger.LogDebug("Registered consumer with tag: {tag}", response.ConsumerTag);

        _consumersByTags.Add(response.ConsumerTag, consumer);
        return response.ConsumerTag;
    }

    public async Task BasicCancel(string consumerTag, bool noWait = false)
    {
        var method = new Basic.Cancel()
        {
            ConsumerTag = consumerTag,
            NoWait = noWait
        };

        if (noWait)
        {
            await CallMethodAsync(method);
            _consumersByTags.Remove(consumerTag);
        }
        else
        {
            var response = await CallMethodAsync<Basic.CancelOk>(method);
            _consumersByTags.Remove(response.ConsumerTag);
        }
    }

    public async Task BasicPublishAsync(string exchange, string routingKey, IMessage message)
    {
        var method = new Basic.Publish
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
            var method = new Basic.Ack
            {
                DeliveryTag = deliveryTag,
                // TODO: add support for this param
                Multiple = false,
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
            var method = new Basic.Reject()
            {
                DeliveryTag = deliveryTag,
                Requeue = requeue
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
        var method = new Basic.Recover()
        {
            Requeue = true
        };
        await CallMethodAsync<Basic.RecoverOk>(method);
    }

    public async Task BasicQos(short prefetchCount, bool global = false)
    {
        var method = new Basic.Qos()
        {
            // Not implemented in rabbitmq
            // https://www.rabbitmq.com/amqp-0-9-1-reference#domain.short
            PrefetchSize = 0,
            PrefetchCount = prefetchCount,
            Global = global
        };

        await CallMethodAsync<Basic.QosOk>(method);
    }

    public async Task<IMessage?> BasicGet(string queue, bool noAck = false)
    {
        var method = new Basic.Get()
        {
            Queue = queue,
            NoAck = noAck
        };

        var result = await CallFrameAsync(method);
        if (result.Method is Basic.GetOk getOkMethod)
        {
            var frame = result.MethodFrame;
            return new IncomingMessage(frame.Body, frame.Properties, getOkMethod.DeliveryTag,
                getOkMethod.Redelivered, getOkMethod.Exchange, getOkMethod.RoutingKey);
        }

        if (result.Method is Basic.GetEmpty) return null;

        throw new Exception($"Unexpected result {result}");
    }

    internal async Task OpenAsync()
    {
        _listenerCancellationSource = new CancellationTokenSource();
        StartListener(_listenerCancellationSource.Token);
        await CallMethodAsync<Channel.OpenOk>(new Channel.Open());
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

                    var frame = (AmqpMethodFrame)(await RxChannel.ReadAsync(cancellationToken));
                    Logger.LogDebug("Frame received:\n\t {method}", frame.Method);

                    if (TryHandleAsyncResponse(frame)) continue;
                    if (TryHandleConsumerMessage(frame)) continue;

                    await HandleIncomingCall(frame);
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
        if (!frame.Method.IsAsyncResponse) return false;

        if (!SyncMethodHandles.TryDequeue(out var result))
            throw new Exception("No task completion source found");

        result.SetResult(new MethodResult(frame));
        return true;
    }

    private async Task HandleIncomingCall(AmqpMethodFrame frame)
    {
        switch (frame.Method)
        {
            case Channel.Close closeMethod:
                Logger.LogError("Closing channel\n {code}, {text} ", closeMethod.ReplyCode, closeMethod.ReplyText);

                var result = new MethodResult(null, closeMethod.ReplyCode, closeMethod.ReplyText);

                if (SyncMethodHandles.TryDequeue(out var source))
                {
                    source.SetResult(result);
                }

                await CallMethodAsync(new Channel.CloseOk());
                LastErrorResult = result;
                State = ChannelState.Failed;
                break;
            default:
                throw new NotImplementedException();
        }
    }
    
    private bool TryHandleConsumerMessage(AmqpMethodFrame frame)
    {
        if (!frame.Method.HasBody || frame.Method is not Basic.Deliver method) return false;

        var message2 = new IncomingMessage(frame.Body, frame.Properties, method.DeliveryTag, method.Redelivered, method.Exchange, method.RoutingKey);
        _consumersByTags[method.ConsumerTag].Invoke(message2);
        return true;
    }
}