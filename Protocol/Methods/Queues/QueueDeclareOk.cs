using AMQPClient.Protocol.Attributes;

namespace AMQPClient.Protocol.Methods.Queues;

[MethodDef(classId: 50, methodId: 11)]
public class QueueDeclareOk : Method
{
    [ShortStringField(0)]
    public string Name { get; set; }
    
    [IntField(1)]
    public int MsgCount { get; set; }
        
    [IntField(2)]
    public int ConsumersCount { get; set; }
}