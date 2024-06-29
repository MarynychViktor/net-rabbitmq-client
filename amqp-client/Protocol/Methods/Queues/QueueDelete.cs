using AMQPClient.Protocol.Attributes;

namespace AMQPClient.Protocol.Methods.Queues;

[MethodDef(50, 40)]
public class QueueDelete : Method
{
    [ShortField(0)] public short Reserved1 { get; set; }

    [ShortStringField(1)] public string Queue { get; set; }

    [ByteField(2)] public byte Flags { get; set; }
}

[Flags]
public enum QueueDeleteFlags
{
    None = 0,
    IfUnused = 1,
    IfEmpty = 2,
    NoWait = 1 << 2,
}