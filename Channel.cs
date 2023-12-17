using System.Collections.Concurrent;
using AMQPClient.Methods.Basic;
using AMQPClient.Methods.Channels;
using AMQPClient.Methods.Exchanges;
using AMQPClient.Protocol;
using Encoder = AMQPClient.Protocol.Encoder;
using Chan = System.Threading.Channels;

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
    public async Task<string> ExchangeDeclare(string name)
    {
        var method = new ExchangeDeclare()
        {
            Name = name,
        };
        await _connection.SendMethodAsync<ExchangeDeclareOk>(ChannelId, method);
        return name;
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

    protected override Type GetMethodType(short classId, short methodId)
    {
        return _methodIdTypeMap[classId * 100 + methodId];
    }

    internal async Task OpenAsync()
    {
        var openMethod = new ChannelOpenMethod();
        // Connection2.SendFrame(new AMQPFrame()
        // {
        //     Type = AMQPFrameType.Method,
        //     Channel = ChannelId,
        //     Body = Encoder.MarshalMethodFrame(openMethod),
        // });
        queue.Take();
    }

    public async Task CloseAsync()
    {
        var closeMethod = new CloseMethod();
        // Connection2.SendFrame(new AMQPFrame()
        // {
        //     Type = AMQPFrameType.Method,
        //     Channel = ChannelId,
        //     Body = Encoder.MarshalMethodFrame(closeMethod),
        // });
        queue.Take();
    }

    public Task HandleFrameAsync(byte type, byte[] body)
    {
        try
        {
            switch (type)
            {
                case 1:
                    HandleMethodFrameAsync(body);
                    break;
                default:
                    throw new Exception($"Not matched type {type}");

            }
            
        }
        catch (Exception e)
        {
            Console.WriteLine($"Exception {e}");
            throw e;
        }

        return Task.CompletedTask;
    }

    private void HandleMethod(ChannelOpenOkMethod m)
    {
        queue.Add(null);
    }

    public void DeclareExchange(string name)
    {
        var declareMethod = new ExchangeDeclare()
        {
            Name = name
        };
        // Connection2.SendFrame(new AMQPFrame()
        // {
        //     Type = AMQPFrameType.Method,
        //     Channel = ChannelId,
        //     Body = Encoder.MarshalMethodFrame(declareMethod),
        // });
        queue.Take();
    }
    
    private void HandleMethod(ExchangeDeclareOk m)
    {
        Console.WriteLine($"Exchange declared {m}");
        queue.Add(null);
    }

    private void HandleMethod(ChannelCloseOkMethod m)
    {
        queue.Add(null);
    }
}