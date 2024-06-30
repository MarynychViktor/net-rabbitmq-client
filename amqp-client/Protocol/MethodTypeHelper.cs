using AMQPClient.Protocol.Method2;
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
    
    private static readonly Dictionary<short, Dictionary<short, Type>> ClassMethodTypeMap2 = new()
    {
        {
            10,
            new()
            {
                { 10, typeof(Method2.Connection.Start) },
                { 30, typeof(Method2.Connection.Tune) },
                { 41, typeof(Method2.Connection.OpenOk) },
                { 50, typeof(Method2.Connection.Close) },
                { 51, typeof(Method2.Connection.CloseOk) },
            }
        },
        {
            20,
            new()
            {
                { 10, typeof(Channel.Open) },
                { 11, typeof(Channel.OpenOk) },
                { 40, typeof(Channel.Close) },
                { 41, typeof(Channel.CloseOk) },
            }
        },
        {
            40,
            new()
            {
                { 10, typeof(Exchange.Declare) },
                { 11, typeof(Exchange.DeclareOk) },
                { 20, typeof(Exchange.Delete) },
                { 21, typeof(Exchange.DeleteOk) },
            }
        },
        {
            50,
            new()
            {
                { 10, typeof(Queue.Declare) },
                { 11, typeof(Queue.DeclareOk) },
                { 20, typeof(Queue.Bind) },
                { 21, typeof(Queue.BindOk) },
                { 30, typeof(Queue.Purge) },
                { 31, typeof(Queue.PurgeOk) },
                { 40, typeof(Queue.Delete) },
                { 41, typeof(Queue.DeleteOk) },
                { 50, typeof(Queue.Unbind) },
                { 51, typeof(Queue.UnbindOk) },
            }
        },
        {
            60,
            new()
            {
                { 10, typeof(Basic.Qos) },
                { 11, typeof(Basic.QosOk) },
                { 20, typeof(Basic.Consume) },
                { 21, typeof(Basic.ConsumeOk) },
                { 30, typeof(Basic.Cancel) },
                { 31, typeof(Basic.CancelOk) },
                { 40, typeof(Basic.Publish) },
                { 60, typeof(Basic.Deliver) },
                { 70, typeof(Basic.Get) },
                { 71, typeof(Basic.GetOk) },
                { 72, typeof(Basic.GetEmpty) },
                { 110, typeof(Basic.Recover) },
                { 111, typeof(Basic.RecoverOk) }
            }
        }
    };
    public static Type GetMethodType2(short classId, short methodId)
    {
        return ClassMethodTypeMap2[classId][methodId];
    }
}