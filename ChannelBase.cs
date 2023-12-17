using System.Reflection;
using AMQPClient.Methods.Connection;
using AMQPClient.Protocol;
using Decoder = AMQPClient.Protocol.Decoder;

namespace AMQPClient;

public abstract class ChannelBase : IAmqpChannel
{
    protected readonly InternalConnection _connection;
    public short ChannelId { get; }

    public ChannelBase(InternalConnection connection, short id)
    {
        _connection = connection;
        ChannelId = id;
    }

    public Task HandleFrameAsync(byte type, byte[] body)
    {
        switch (type)
        {
            case 1:
                HandleMethodFrameAsync(body);
                break;
            default:
                throw new Exception($"Not matched type {type}");
        }

        return Task.CompletedTask;
    }

    public abstract Task HandleFrameAsync(LowLevelAmqpMethodFrame frame);

    protected abstract Type GetMethodType(short classId, short methodId);
    
    public Task HandleMethodFrameAsync(byte[] body)
    {
        try
        {
            var reader = new BinReader(body);
            var classId = reader.ReadInt16();
            var methodId = reader.ReadInt16();
            var methodType = GetMethodType(classId, methodId);

            if (methodType == null)
            {
                throw new Exception($"Not registered method type for {classId} {methodId}");
            }
            
            var methodBody = typeof(Decoder).GetMethod("UnmarshalMethodFrame")
                .MakeGenericMethod(methodType)
                .Invoke(this, new []{body});

            var handlerMethod = GetType().GetMethod(
                    "HandleMethod",
                    BindingFlags.NonPublic | BindingFlags.Instance,
                    new []{methodType}
                );

            if (handlerMethod == null)
            {
                throw new Exception($"Failed to resolve handler for method with classId: {classId}, methodId: {methodId}");
            }

            handlerMethod.Invoke(this, new []{ methodBody });
        }
        catch (Exception e)
        {
            Console.WriteLine($"Exception {e}");
            throw e;
        }

        return Task.CompletedTask;
    }
}