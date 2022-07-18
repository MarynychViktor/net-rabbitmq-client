using System.Reflection;
using System.Text;
using AMQPClient.Methods.Connection;
using AMQPClient.Protocol;
using Decoder = AMQPClient.Protocol.Decoder;
using Encoder = AMQPClient.Protocol.Encoder;

namespace AMQPClient;

public class DefaultAmqpChannel : IAmqpChannel
{
    private readonly Connection _connection;

    private readonly Dictionary<int, Type> _methodIdTypeMap = new ()
    {
        {1010, typeof(StartMethod)},
        {1030, typeof(TuneMethod)},
        {1041, typeof(OpenOkMethod)},
    };

    public DefaultAmqpChannel(Connection connection)
    {
        _connection = connection;
    }

    public Task HandleFrameAsync(byte type, byte[] body)
    {
        Console.WriteLine($"Handle frame async called {body.Length}");

        try
        {
            switch (type)
            {
                case 1:
                    Console.WriteLine("HandleMethodFrame");
                    HandleMethodFrameAsync(body);
                    break;
                default:
                    throw new Exception($"Not matched type {type}");

            }
            
        }
        catch (Exception e)
        {
            Console.WriteLine($"Exception {e}");
        }

        return Task.CompletedTask;
    }
    
    
    public Task HandleMethodFrameAsync(byte[] body)
    {
        try
        {
            var sb = new StringBuilder();
            sb.Append("[");
            foreach (var b in body)
            {
                sb.Append(b + ",");
            }

            sb.Append("]");
            Console.WriteLine("11111111111111");
            Console.WriteLine(sb.ToString());
            Console.WriteLine("11111111111111");
            var reader = new BinReader(body);
            
            short classId = reader.ReadInt16();
            short methodId = reader.ReadInt16();

            var methodType = _methodIdTypeMap[classId * 100 + methodId];

            if (methodType == null)
            {
                throw new Exception($"Not registered method type for {classId} {methodId}");
            }
            
            var methodObj = typeof(Decoder).GetMethod("UnmarshalMethodFrame").MakeGenericMethod(methodType)
                .Invoke(this, new []{body});

            GetType().GetMethod(
                    "HandleMethod",
                    BindingFlags.NonPublic | BindingFlags.Instance,
                    new []{methodType}
                )
                .Invoke(this, new []{methodObj});

            // throw new Exception("reached place of dynamic call");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Exception {e}");
        }

        return Task.CompletedTask;
    }

    private void HandleMethod(StartMethod m)
    {
        var startOkMethod = new StartOkMethod()
        {
            ClientProperties = new Dictionary<string, object>()
            {
                { "product", "simpleapp123123asd asd asdasd simpleapp123123asd asd asdasd" },
                { "platform", "Erlang/OTP 24.3.4" },
                { "copyright", "param-pam-pamqweqwe" },
                { "information", "Licensed under the MPL 2.0" },
            },
            Mechanism = "PLAIN",
            Response = "\x00" + "user" + "\x00" + "password",
            Locale = "en_US",
        };
        var bytes = Encoder.MarshalMethodFrame(startOkMethod);
        var bts = Decoder.UnmarshalMethodFrame<StartOkMethod>(bytes);
        Console.WriteLine($"----bts ${bytes.Length}");
        Console.WriteLine(bts);

        var frm = new AMQPFrame()
        {
            Type = AMQPFrameType.Method,
            Channel = 0,
            Body = bytes
        }.ToBytes();
        var sb = new StringBuilder();
        sb.Append("[");
        foreach (var b in frm)
        {
            sb.Append(b + ",");
        }

        sb.Append("]");
        Console.WriteLine("------------");
        Console.WriteLine(sb.ToString());
        Console.WriteLine("------------");

        _connection._client.Client.Send(frm);
        Console.WriteLine("****");
        Console.WriteLine("Conn-ok sent");
    }
      
    private void HandleMethod(TuneMethod m)
    {
        var tuneOkMethod = new TuneOkMethod()
        {
            ChannelMax = m.ChannelMax,
            Heartbeat = m.Heartbeat,
            FrameMax = m.FrameMax,
        };
        var bytes = Encoder.MarshalMethodFrame(tuneOkMethod);
        _connection._client.Client.Send(new AMQPFrame()
        {
            Type = AMQPFrameType.Method,
            Channel = 0,
            Body = bytes
        }.ToBytes());
        
        var openMethod = new OpenMethod()
        {
            VirtualHost = "my_vhost"
        };
        bytes = Encoder.MarshalMethodFrame(openMethod);
        _connection._client.Client.Send(new AMQPFrame()
        {
            Type = AMQPFrameType.Method,
            Channel = 0,
            Body = bytes
        }.ToBytes());
        // var startOkMethod = new StartOkMethod()
        // {
        //     ClientProperties = new Dictionary<string, object>()
        //     {
        //         { "product", "simpleapp123123asd asd asdasd simpleapp123123asd asd asdasd" },
        //         { "platform", "Erlang/OTP 24.3.4" },
        //         { "copyright", "param-pam-pamqweqwe" },
        //         { "information", "Licensed under the MPL 2.0" },
        //     },
        //     Mechanism = "PLAIN",
        //     Response = "\x00" + "user" + "\x00" + "password",
        //     Locale = "en_US",
        // };
        // var bytes = Encoder.MarshalMethodFrame(startOkMethod);
        // var bts = Decoder.UnmarshalMethodFrame<StartOkMethod>(bytes);
        // Console.WriteLine($"----bts ${bytes.Length}");
        // Console.WriteLine(bts);
        //
        // var frm = new AMQPFrame()
        // {
        //     Type = AMQPFrameType.Method,
        //     Channel = 0,
        //     Body = bytes
        // }.ToBytes();
        // var sb = new StringBuilder();
        // sb.Append("[");
        // foreach (var b in frm)
        // {
        //     sb.Append(b + ",");
        // }
        //
        // sb.Append("]");
        // Console.WriteLine("------------");
        // Console.WriteLine(sb.ToString());
        // Console.WriteLine("------------");
        //
        // _connection._client.Client.Send(frm);
        // Console.WriteLine("****");
        // Console.WriteLine("Conn-ok sent");
    }
private void HandleMethod(OpenOkMethod m)
{
    Console.WriteLine($"Open-ok received {m}");
    
}
    private void HandleMethod(object m)
    {
        Console.WriteLine("Handing Object method");
    }
}