using System.Collections.Concurrent;
using System.Text;
using AMQPClient.Methods.Basic;
using AMQPClient.Methods.Channels;
using AMQPClient.Methods.Exchanges;
using AMQPClient.Protocol;
using AMQPClient.Types;
using Encoder = AMQPClient.Protocol.Encoder;

namespace AMQPClient;

public class Channel : ChannelBase
{
    private BlockingCollection<object> queue = new ();
    public Dictionary<string, Action<AmqpEnvelope>> BasicConsumers = new();

    public Channel(InternalConnection connection, short id) : base(connection, id)
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

    public async Task BasicPublish(string exchange, string routingKey, HeaderProperties properties, String body)
    {
        var method = new BasicPublish()
        {
            Exchange = exchange,
            RoutingKey = routingKey,
        };

        // var properties = new HeaderProperties();
        // var body = envelope.Payload!.Content;
        // var methodFrame = new LowLevelAmqpMethodFrame(ChannelId, method);
        // var headerFrame = new LowLevelAmqpHeaderFrame(ChannelId, method.ClassMethodId().Item1, body.Length, properties);
        // var bodyFrame = new LowLevelAmqpBodyFrame(ChannelId, body);

        var envelopePayload = new AmqpEnvelopePayload(properties, Encoding.UTF8.GetBytes(body));
        var envelope = new AmqpEnvelope(method, envelopePayload);

        await _connection.SendEnvelopeAsync(ChannelId, envelope);

        // var response = await _connection.SendMethodAsync<BasicConsumeOk>(ChannelId, method);
        // Console.WriteLine($"Registered consumer with tag{response.Tag}");
        // BasicConsumers.Add(response.Tag, consumer);
    }
    
    public override Task HandleFrameAsync(LowLevelAmqpMethodFrame frame)
    {
        var (classId, methodId) = frame.Method.ClassMethodId();
        // FIXME:
        var methodType = AmpqMethodMap.GetMethodType(classId, methodId);

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