using AMQPClient.Protocol.Attributes;

namespace AMQPClient.Protocol.Methods.Queues;

[MethodDef(50, 31)]
public class QueuePurgeOk : Method
{
    [IntField(0)]
    public int MessageCount { get; set; }
}