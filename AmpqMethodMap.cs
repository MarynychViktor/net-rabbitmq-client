using AMQPClient.Methods.Channels;
using AMQPClient.Methods.Connection;

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
    };

    private static readonly IReadOnlyList<Type> ResponseMethods = new List<Type>()
    {
        typeof(ChannelOpenOkMethod),
    };

    public static bool IsAsyncResponse(short classId, short methodId) => ResponseMethods.Contains(GetMethodType(classId, methodId));

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