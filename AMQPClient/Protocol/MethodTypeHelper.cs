using AMQPClient.Protocol.Classes;

namespace AMQPClient.Protocol;

public static class MethodTypeHelper
{
    private static readonly Dictionary<short, Dictionary<short, Type>> ClassMethodTypeMap = new()
    {
        {
            10,
            new()
            {
                { 10, typeof(Classes.Connection.Start) },
                { 30, typeof(Classes.Connection.Tune) },
                { 41, typeof(Classes.Connection.OpenOk) },
                { 50, typeof(Classes.Connection.Close) },
                { 51, typeof(Classes.Connection.CloseOk) },
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
    public static Type GetMethodType(short classId, short methodId)
    {
        return ClassMethodTypeMap[classId][methodId];
    }
}