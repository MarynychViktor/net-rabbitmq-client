using AMQPClient.Protocol.Attributes;

namespace AMQPClient.Protocol.Methods.Queues;

[MethodDef(50, 41)]
public class QueueDeleteOk : Method
{
    [IntField(0)] public int MessageCount { get; set; }
}
