using AMQPClient.Protocol.Methods.Basic;
using AMQPClient.Protocol.Methods.Channels;
using AMQPClient.Protocol.Methods.Connection;
using AMQPClient.Protocol.Methods.Exchanges;
using AMQPClient.Protocol.Methods.Queues;

namespace AMQPClient.Protocol;

public static class MethodMetaRegistry
{
    private static readonly Dictionary<short, Type> _methodIdTypeMap = new()
    {
        { 1010, typeof(StartMethod) },
        { 1030, typeof(TuneMethod) },
        { 1041, typeof(OpenOkMethod) },
        { 1050, typeof(ConnectionClose) },
        { 1051, typeof(ConnectionCloseOk) },
        // Channel
        { 2010, typeof(ChannelOpenMethod) },
        { 2011, typeof(ChannelOpenOkMethod) },
        { 2040, typeof(ChannelCloseMethod) },
        { 2041, typeof(ChannelCloseOkMethod) },
        // Exchange
        { 4010, typeof(ExchangeDeclare) },
        { 4011, typeof(ExchangeDeclareOk) },
        { 4020, typeof(ExchangeDelete) },
        { 4021, typeof(ExchangeDeleteOk) },
        // Queue
        { 5010, typeof(QueueDeclare) },
        { 5011, typeof(QueueDeclareOk) },
        { 5020, typeof(QueueBind) },
        { 5021, typeof(QueueBindOk) },
        { 5030, typeof(QueuePurge) },
        { 5031, typeof(QueuePurgeOk) },
        { 5040, typeof(QueueDelete) },
        { 5041, typeof(QueueDeleteOk) },
        { 5050, typeof(QueueUnbind) },
        { 5051, typeof(QueueUnbindOk) },
        // Basic
        { 6020, typeof(BasicConsume) },
        { 6021, typeof(BasicConsumeOk) },
        { 6030, typeof(BasicCancel) },
        { 6031, typeof(BasicCancelOk) },
        { 6040, typeof(BasicPublish) },
        { 6060, typeof(BasicDeliver) },
        { 6110, typeof(BasicRecover) },
        { 6111, typeof(BasicRecoverOk) }
    };

    private static readonly IReadOnlyList<Type> ResponseMethods = new List<Type>
    {
        typeof(ChannelOpenOkMethod),
        typeof(ExchangeDeclareOk),
        typeof(ExchangeDeleteOk),
        typeof(QueueDeclareOk),
        typeof(QueueBindOk),
        typeof(BasicConsumeOk),
        typeof(ConnectionCloseOk),
        typeof(BasicCancelOk),
        typeof(QueueUnbindOk),
        typeof(QueueDeleteOk),
        typeof(QueuePurgeOk),
        typeof(BasicRecoverOk),
    };

    private static readonly IReadOnlyList<Type> MethodsWithBody = new List<Type>
    {
        typeof(BasicDeliver),
        typeof(BasicPublish)
    };

    public static bool IsAsyncResponse(short classId, short methodId)
    {
        return ResponseMethods.Contains(GetMethodType(classId, methodId));
    }

    public static bool HasBody(short classId, short methodId)
    {
        return MethodsWithBody.Contains(GetMethodType(classId, methodId));
    }

    public static Type GetMethodType(short classId, short methodId)
    {
        return _methodIdTypeMap[(short)(classId * 100 + methodId)];
    }
}