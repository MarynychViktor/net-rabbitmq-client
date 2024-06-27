using System.Threading.Channels;
using AMQPClient.Protocol;
using AMQPClient.Protocol.Methods.Basic;
using AMQPClient.Protocol.Methods.Channels;
using AMQPClient.Protocol.Methods.Exchanges;
using AMQPClient.Protocol.Methods.Queues;
using Microsoft.Extensions.Logging;

namespace AMQPClient;

public class ChannelImpl(Channel<object> trxChannel, IAmqpFrameSender frameSender, short id)
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
        IsClosed = true;
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

        throw new NotImplementedException();
    }

    internal async Task OpenAsync(short channelId)
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

                            if (frame.Method.IsAsyncResponse())
                            {
                                if (!SyncMethodHandles[ChannelId]
                                        .TryDequeue(out var result))
                                    throw new Exception("No task completion source found");

                                result.SetResult(frame);
                                break;
                            }

                            if (!frame.Method.HasBody() || frame.Method is not BasicDeliver method)
                                throw new NotImplementedException();

                            var message2 = new IncomingMessage(frame.Body, frame.Properties, method.DeliverTag,
                                method.Redelivered == 1, method.Exchange, method.RoutingKey);
                            _consumersByTags[method.ConsumerTag].Invoke(message2);
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
}