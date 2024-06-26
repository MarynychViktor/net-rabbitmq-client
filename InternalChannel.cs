using System.Threading.Channels;
using AMQPClient.Protocol;
using AMQPClient.Protocol.Methods.Basic;
using AMQPClient.Protocol.Methods.Channels;
using AMQPClient.Protocol.Methods.Exchanges;
using AMQPClient.Protocol.Methods.Queues;
using AMQPClient.Protocol.Types;
using Microsoft.Extensions.Logging;

namespace AMQPClient;

public class InternalChannel : ChannelBase
{
    public Dictionary<string, Action<AmqpEnvelope>> BasicConsumers = new();
    private readonly Channel<object> _trxChannel;
    private ChannelReader<object> RxChannel => _trxChannel.Reader;
    private readonly MethodCaller _methodCaller;
    private ILogger<InternalChannel> Logger { get; } = DefaultLoggerFactory.CreateLogger<InternalChannel>();

    public InternalChannel(Channel<object> trxChannel, MethodCaller methodCaller, InternalConnection connection, short id) : base(connection, id)
    {
        _trxChannel = trxChannel;
        _methodCaller = methodCaller;
    }

    public async Task OpenAsync(short channelId)
    {        
        StartListener();
        await _methodCaller.CallMethodAsync<ChannelOpenOkMethod>(channelId, new ChannelOpenMethod());
    }

    public void StartListener(CancellationToken cancellationToken = default)
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
                            if (!_methodCaller.SyncCallResponseWaitQueue[ChannelId]
                                .TryDequeue(out TaskCompletionSource<AmqpMethodFrame> result))
                            {
                                throw new Exception("No task completion source found");
                            }
   
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
                            {
                                BasicConsumers[method.ConsumerTag].Invoke(envelope);
                            }
                        }
                        break;
                    default:
                        throw new Exception("Unknown frame");
                }
            }

            return Task.CompletedTask;
        }, cancellationToken);
    }

    public async Task ExchangeDeclare(string name, bool passive = false, bool durable = false, bool autoDelete = false, bool internalOnly = false, bool nowait = false)
    {
        var flags = ExchangeDeclareFlags.None;
        if (passive)
        {
            flags &= ExchangeDeclareFlags.Passive;
        }

        if (durable)
        {
            flags &= ExchangeDeclareFlags.Durable;
        }

        if (autoDelete)
        {
            flags &= ExchangeDeclareFlags.AutoDelete;
        }

        if (internalOnly)
        {
            flags &= ExchangeDeclareFlags.Internal;
        }

        if (nowait)
        {
            // FIXME: implement nowait
            // flags &= ExchangeDeclareFlags.NoWait;
        }

        var method = new ExchangeDeclare()
        {
            Name = name,
            Flags = (byte) flags,
        };

        await _methodCaller.CallMethodAsync<ExchangeDeclareOk>(ChannelId, method);
    }

    public async Task ExchangeDelete(string name)
    {
        var method = new ExchangeDelete()
        {
            Name = name,
        };

        await _methodCaller.CallMethodAsync<ExchangeDeleteOk>(ChannelId, method);
    }
    
    // FIXME: add actual params to method
    public async Task<string> QueueDeclare(string name = "")
    {
        var method = new QueueDeclare()
        {
            Name = name,
        };

        var result = await _methodCaller.CallMethodAsync<QueueDeclareOk>(ChannelId, method);
        return result.Name;
    }

    // FIXME: add actual params to method
    public async Task QueueBind(string queue, string exchange, string routingKey)
    {
        var method = new QueueBind()
        {
            Queue = queue,
            Exchange = exchange,
            RoutingKey = routingKey,
        };
        await _methodCaller.CallMethodAsync<QueueBindOk>(ChannelId, method);
    }
   
    // FIXME: add actual params to method
    public async Task BasicConsume(string queue, Action<AmqpEnvelope> consumer)
    {
        var method = new BasicConsume()
        {
            Queue = queue,
        };
        var response = await _methodCaller.CallMethodAsync<BasicConsumeOk>(ChannelId, method);
        Console.WriteLine($"Registered consumer with tag{response.Tag}");
        BasicConsumers.Add(response.Tag, consumer);
    }

    public Task BasicPublishAsync(string exchange, string routingKey, Message message)
    {
        var method = new BasicPublish()
        {
            Exchange = exchange,
            RoutingKey = routingKey,
        };

        var envelopePayload = new AmqpEnvelopePayload(message.Properties, message.Data);
        var envelope = new AmqpEnvelope(method, envelopePayload);

        return _connection.SendEnvelopeAsync(ChannelId, envelope);
    }

    public async Task BasicAck(AmqpEnvelope message)
    {
        if (message.Method is BasicDeliver deliverMethod)
        {
            var method = new BasicAck()
            {
                Tag = deliverMethod.DeliverTag,
                Multiple = 0,
            };

            await _methodCaller.CallMethodAsync(ChannelId, method);
            
            return;
        }
        
        throw new NotImplementedException();
    }

    public override Task HandleFrameAsync(AmqpMethodFrame frame)
    {
        var (classId, methodId) = frame.Method.ClassMethodId();
        // FIXME:
        var methodType = MethodMetaRegistry.GetMethodType(classId, methodId);

        if (methodType == typeof(BasicDeliver))
        {
            var method = (BasicDeliver)frame.Method;
            Console.WriteLine("Basic deliver received");
            // BasicConsumers[method.ConsumerTag].Invoke(frame);
            return Task.CompletedTask;
        }

        throw new NotImplementedException();
    }

    public override Task HandleEnvelopeAsync(AmqpEnvelope envelope)
    {
        if (envelope.Method is BasicDeliver method)
        {
            Console.WriteLine("Basic deliver received123");
            BasicConsumers[method.ConsumerTag].Invoke(envelope);
            return Task.CompletedTask;
        }
        
        throw new NotImplementedException();
    }

    public async override Task HandleEvent<T>(InternalEvent<T> @event)
    {
        switch (@event)
        {
            case InternalEvent<AmqpMethodFrame>:
                Console.WriteLine("Method frame received");
                return;
            case InternalEvent<AmqpHeaderFrame>:
                Console.WriteLine("Header frame received");
                return;
            case InternalEvent<AmqpBodyFrame>:
                Console.WriteLine("Body frame received");
                return;
        }
    }
}