using AMQPClient.Protocol.Methods.Basic;
using AMQPClient.Protocol.Methods.Channels;
using AMQPClient.Protocol.Methods.Connection;
using AMQPClient.Protocol.Methods.Exchanges;
using AMQPClient.Protocol.Methods.Queues;

namespace AMQPClient.Protocol;

public static class MethodTypeHelper
{
    private static readonly Dictionary<short, Dictionary<short, Type>> ClassMethodTypeMap = new()
    {
        {
            10,
            new()
            {
                { 10, typeof(StartMethod) },
                { 30, typeof(TuneMethod) },
                { 41, typeof(OpenOkMethod) },
                { 50, typeof(ConnectionClose) },
                { 51, typeof(ConnectionCloseOk) },
            }
        },
        {
            20,
            new()
            {
                { 10, typeof(ChannelOpenMethod) },
                { 11, typeof(ChannelOpenOkMethod) },
                { 40, typeof(ChannelCloseMethod) },
                { 41, typeof(ChannelCloseOkMethod) },
            }
        },
        {
            40,
            new()
            {
                { 10, typeof(ExchangeDeclare) },
                { 11, typeof(ExchangeDeclareOk) },
                { 20, typeof(ExchangeDelete) },
                { 21, typeof(ExchangeDeleteOk) },
            }
        },
        {
            50,
            new()
            {
                { 10, typeof(QueueDeclare) },
                { 11, typeof(QueueDeclareOk) },
                { 20, typeof(QueueBind) },
                { 21, typeof(QueueBindOk) },
                { 30, typeof(QueuePurge) },
                { 31, typeof(QueuePurgeOk) },
                { 40, typeof(QueueDelete) },
                { 41, typeof(QueueDeleteOk) },
                { 50, typeof(QueueUnbind) },
                { 51, typeof(QueueUnbindOk) },
            }
        },
        {
            60,
            new()
            {
                { 10, typeof(BasicQos) },
                { 11, typeof(BasicQosOk) },
                { 20, typeof(BasicConsume) },
                { 21, typeof(BasicConsumeOk) },
                { 30, typeof(BasicCancel) },
                { 31, typeof(BasicCancelOk) },
                { 40, typeof(BasicPublish) },
                { 60, typeof(BasicDeliver) },
                { 70, typeof(BasicGet) },
                { 71, typeof(BasicGetOk) },
                { 72, typeof(BasicGetEmpty) },
                { 110, typeof(BasicRecover) },
                { 111, typeof(BasicRecoverOk) }
            }
        }
    };

    public static Type GetMethodType(short classId, short methodId)
    {
        return ClassMethodTypeMap[classId][methodId];
    }
}