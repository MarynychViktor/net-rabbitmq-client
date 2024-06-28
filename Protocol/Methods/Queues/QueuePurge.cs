using AMQPClient.Protocol.Attributes;

namespace AMQPClient.Protocol.Methods.Queues;

[MethodDef(50, 30)]
public class QueuePurge : Method
{
    [ShortField(0)]
    public short Reserved1 { get; set; }

    [ShortStringField(1)]
    public string Queue { get; set; }

    [ByteField(2)]
    public byte NoWait { get; set; }
}