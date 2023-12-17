using AMQPClient.Methods.Basic;
using AMQPClient.Methods.Channels;
using AMQPClient.Methods.Connection;
using AMQPClient.Methods.Exchanges;

namespace AMQPClient;

public static class AmpqMethodMap
{
    private static readonly Dictionary<short, Type> _methodIdTypeMap = new ()
    {
        {1010, typeof(StartMethod)},
        {1030, typeof(TuneMethod)},
        {1041, typeof(OpenOkMethod)},
        {1050, typeof(ConnectionClose)},
        {2010, typeof(ChannelOpenMethod)},
        {2011, typeof(ChannelOpenOkMethod)},
        // Exchange
        {4010, typeof(ExchangeDeclare)},
        {4011, typeof(ExchangeDeclareOk)},
        {4020, typeof(ExchangeDelete)},
        {4021, typeof(ExchangeDeleteOk)},
        // Queue
        {5010, typeof(QueueDeclare)},
        {5011, typeof(QueueDeclareOk)},
        {5020, typeof(QueueBind)},
        {5021, typeof(QueueBindOk)},
        // Basic
        {6020, typeof(BasicConsume)},
        {6021, typeof(BasicConsumeOk)},
        {6060, typeof(BasicDeliver)},
    };

    private static readonly IReadOnlyList<Type> ResponseMethods = new List<Type>()
    {
        typeof(ChannelOpenOkMethod),
        typeof(ExchangeDeclareOk),
        typeof(ExchangeDeleteOk),
        typeof(QueueDeclareOk),
        typeof(QueueBindOk),
        typeof(BasicConsumeOk),
    };

    private static readonly IReadOnlyList<Type> MethodsWithBody = new List<Type>()
    {
        typeof(BasicDeliver),
    };

    public static bool IsAsyncResponse(short classId, short methodId) => ResponseMethods.Contains(GetMethodType(classId, methodId));
    public static bool HasBody(short classId, short methodId) => MethodsWithBody.Contains(GetMethodType(classId, methodId));

    public static Type GetMethodType(short classId, short methodId)
    {
        return _methodIdTypeMap[(short) (classId * 100 + methodId)];
    }

    public static (short, short) GetMethodId(Type methodType)
    {
        foreach (var keyValuePair in _methodIdTypeMap)
        {
            if (keyValuePair.Value == methodType)
            {
                var classId = (short) (keyValuePair.Key % 1000);
                var methodId = (short) (keyValuePair.Key / 100);
                return (classId, methodId);
            }
        }

        throw new ArgumentOutOfRangeException($"Invalid method type provided {methodType}");
    }
}