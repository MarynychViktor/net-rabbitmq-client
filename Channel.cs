using System.Collections.Concurrent;
using AMQPClient.Methods.Basic;
using AMQPClient.Methods.Channels;
using AMQPClient.Methods.Exchanges;
using AMQPClient.Protocol;

namespace AMQPClient;

public class Channel : ChannelBase
{
    private BlockingCollection<object> queue = new ();
    public Dictionary<string, Action<LowLevelAmqpMethodFrame>> BasicConsumers = new();

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
    public async Task BasicConsume(string queue, Action<LowLevelAmqpMethodFrame> consumer)
    {
        var method = new BasicConsume()
        {
            Queue = queue,
        };
        var response = await _connection.SendMethodAsync<BasicConsumeOk>(ChannelId, method);
        Console.WriteLine($"Registered consumer with tag{response.Tag}");
        BasicConsumers.Add(response.Tag, consumer);
    }

    public override Task HandleFrameAsync(LowLevelAmqpMethodFrame frame)
    {
        var methodType = AmpqMethodMap.GetMethodType(frame.ClassId, frame.MethodId);

        if (methodType == typeof(BasicDeliver))
        {
            var method = frame.castTo<BasicDeliver>();
            Console.WriteLine("Basic deliver received");
            BasicConsumers[method.ConsumerTag].Invoke(frame);
            return Task.CompletedTask;
        }

        throw new NotImplementedException();
    }
}