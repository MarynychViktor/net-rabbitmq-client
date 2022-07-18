using AMQPClient.Methods.Channels;
using AMQPClient.Protocol;
using Encoder = AMQPClient.Protocol.Encoder;
using Chan = System.Threading.Channels;

namespace AMQPClient;

public class Channel : ChannelBase
{
    private Chan.Channel<byte> _openChannel;

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
        _openChannel = Chan.Channel.CreateBounded<byte>(1);
        Connection.SendFrame(new AMQPFrame()
        {
            Type = AMQPFrameType.Method,
            Channel = ChannelId,
            Body = Encoder.MarshalMethodFrame(openMethod),
        });
        await _openChannel.Reader.ReadAsync();
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
        _openChannel.Writer.WriteAsync(0).GetAwaiter().GetResult();
    }
}