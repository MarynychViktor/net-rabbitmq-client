using System.Collections.Concurrent;
using System.Text;
using AMQPClient.Protocol;
using AMQPClient.Protocol.Methods.Basic;
using AMQPClient.Protocol.Methods.Channels;
using AMQPClient.Protocol.Methods.Exchanges;
using AMQPClient.Protocol.Methods.Queues;
using AMQPClient.Protocol.Types;

namespace AMQPClient;

public class InternalChannel : ChannelBase
{
    private BlockingCollection<object> queue = new ();
    public Dictionary<string, Action<AmqpEnvelope>> BasicConsumers = new();

    public InternalChannel(InternalConnection connection, short id) : base(connection, id)
    { }

    private readonly Dictionary<int, Type> _methodIdTypeMap = new ()
    {
        {2011, typeof(ChannelOpenOkMethod)},
        {2041, typeof(ChannelCloseOkMethod)},
        {4011, typeof(ExchangeDeclareOk)},
    };

    // FIXME: add actual params to method
    public async Task ExchangeDeclare(string name, bool passive = false, bool durable = false, bool autoDelete = false, bool internal_only = false, bool nowait = false)
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

        if (internal_only)
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

        await _connection.SendMethodAsync<ExchangeDeclareOk>(ChannelId, method);
    }

    public async Task ExchangeDelete(string name)
    {
        var method = new ExchangeDelete()
        {
            Name = name,
        };
        await _connection.SendMethodAsync<ExchangeDeleteOk>(ChannelId, method);
    }
    
    // FIXME: add actual params to method
    public async Task<string> QueueDeclare(string name = "")
    {
        var method = new QueueDeclare()
        {
            Name = name,
        };
        var response = await _connection.SendMethodAsync<QueueDeclareOk>(ChannelId, method);
        return response.Name;
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
        await _connection.SendMethodAsync<QueueBindOk>(ChannelId, method);
    }
   
    // FIXME: add actual params to method
    public async Task BasicConsume(string queue, Action<AmqpEnvelope> consumer)
    {
        var method = new BasicConsume()
        {
            Queue = queue,
        };
        var response = await _connection.SendMethodAsync<BasicConsumeOk>(ChannelId, method);
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

            await _connection.SendMethodAsync(ChannelId, method);
            
            return;
        }
        
        throw new NotImplementedException();
    }
 
    // public Task BasicPublish(string exchange, string routingKey, HeaderProperties properties, byte[] body)
    // {
    //     var method = new BasicPublish()
    //     {
    //         Exchange = exchange,
    //         RoutingKey = routingKey,
    //     };
    //
    //     var envelopePayload = new AmqpEnvelopePayload(properties, body);
    //     var envelope = new AmqpEnvelope(method, envelopePayload);
    //
    //     return _connection.SendEnvelopeAsync(ChannelId, envelope);
    // }
    
    public override Task HandleFrameAsync(LowLevelAmqpMethodFrame frame)
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
            Console.WriteLine("Basic deliver received");
            BasicConsumers[method.ConsumerTag].Invoke(envelope);
            return Task.CompletedTask;
        }
        
        throw new NotImplementedException();
    }
}