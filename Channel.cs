using System.Net.Sockets;
using AMQPClient.Methods.Channels;
using AMQPClient.Protocol;

namespace AMQPClient;

using Chan = System.Threading.Channels;

public class Channel : ChannelBase
{
    private Chan.Channel<byte> _openChannel;

    public Channel(Connection connection, short id) : base(connection, id)
    {
    }
    private readonly Dictionary<int, Type> _methodIdTypeMap = new ()
    {
        {2011, typeof(OpenOkMethod)},
    };

    protected override Type GetMethodType(short classId, short methodId)
    {
        return _methodIdTypeMap[classId * 100 + methodId];
    }
    internal async Task OpenAsync()
    {
        var openMethod = new OpenMethod();
        _openChannel = Chan.Channel.CreateBounded<byte>(1);

        await _openChannel.Reader.ReadAsync();

        Connection.SendFrame(new AMQPFrame()
        {
            Type = AMQPFrameType.Method,
            Channel = ChannelId,
            Body = Encoder.MarshalMethodFrame(openMethod),
        });
    }

    public Task HandleFrameAsync(byte type, byte[] body)
    {
        Console.WriteLine($"Handle frame async called {body.Length}");

        try
        {
            switch (type)
            {
                case 1:
                    Console.WriteLine("Channel ---HandleMethodFrame");
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


    private void HandleMethod(OpenOkMethod m)
    {
        Console.WriteLine($"Channel Open-ok received {m}");
        _openChannel.Writer.WriteAsync(0).GetAwaiter().GetResult();
        Console.WriteLine($"Channel Open-ok received {m}");
    }
}