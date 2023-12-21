using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using AMQPClient.Methods.Connection;
using AMQPClient.Protocol;
using AMQPClient.Types;
using Decoder = AMQPClient.Protocol.Decoder;
using Encoder = AMQPClient.Protocol.Encoder;

namespace AMQPClient;

public class DefaultAmqpChannel : ChannelBase
{
    private readonly Dictionary<int, Type> _methodIdTypeMap = new ()
    {
        {1010, typeof(StartMethod)},
        {1030, typeof(TuneMethod)},
        {1041, typeof(OpenOkMethod)},
        {1050, typeof(ConnectionClose)},
    };
    private BlockingCollection<object> queue = new ();

    public override Task HandleFrameAsync(LowLevelAmqpMethodFrame frame)
    {
        throw new NotImplementedException();
    }

    public override Task HandleEnvelopeAsync(AmqpEnvelope envelope)
    {
        throw new NotImplementedException();
    }

    public void SendProtocolHeader()
    {
        // Connection.Send(Encoding.ASCII.GetBytes("AMQP").Concat(new byte[] { 0, 0, 9, 1 }).ToArray());
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
        var frame = new AMQPFrame()
        {
            Type = AMQPFrameType.Method,
            Channel = 0,
            Body = bytes
        };

        // Connection.SendFrame(frame);
    }
      
    private void HandleMethod(TuneMethod m)
    {
        var tuneOkMethod = new TuneOkMethod()
        {
            ChannelMax = m.ChannelMax,
            Heartbeat = m.Heartbeat,
            FrameMax = m.FrameMax,
        };
        // Connection.SendFrame(new AMQPFrame()
        // {
        //     Type = AMQPFrameType.Method,
        //     Channel = 0,
        //     Body = Encoder.MarshalMethodFrame(tuneOkMethod)
        // });
        //
        // var openMethod = new OpenMethod()
        // {
        //     VirtualHost = "my_vhost"
        // };
        // Connection.SendFrame(new AMQPFrame()
        // {
        //     Type = AMQPFrameType.Method,
        //     Channel = 0,
        //     Body =  Encoder.MarshalMethodFrame(openMethod)
        // });
    }

    private void HandleMethod(OpenOkMethod m)
    {
        // Connection.OpenEnd();
    }

    private void HandleMethod(ConnectionClose m)
    {
        Console.WriteLine("ConnectionClose");
        Console.WriteLine(m);
    }

    public DefaultAmqpChannel(InternalConnection connection) : base(connection, 0)
    {
    }
}