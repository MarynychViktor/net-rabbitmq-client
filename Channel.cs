using System.Collections.Concurrent;
using AMQPClient.Methods.Channels;
using AMQPClient.Protocol;
using Encoder = AMQPClient.Protocol.Encoder;
using Chan = System.Threading.Channels;

namespace AMQPClient;

public class Channel : ChannelBase
{
    private BlockingCollection<object> queue = new ();

    public Channel(Connection connection, short id) : base(connection, id)
    { }

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
        Connection.SendFrame(new AMQPFrame()
        {
            Type = AMQPFrameType.Method,
            Channel = ChannelId,
            Body = Encoder.MarshalMethodFrame(openMethod),
        });
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


    private void HandleMethod(OpenOkMethod m)
    {
        queue.Add(null);
    }
}