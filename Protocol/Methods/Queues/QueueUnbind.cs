using AMQPClient.Protocol.Attributes;

namespace AMQPClient.Protocol.Methods.Queues;

[MethodDef(50, 50)]
public class QueueUnbind : Method
{
    [ShortField(0)] public short Reserved1 { get; set; }

    [ShortStringField(1)] public string Queue { get; set; }

    [ShortStringField(2)] public string Exchange { get; set; }

    [ShortStringField(3)] public string RoutingKey { get; set; }

    [PropertiesTableField(4)] public Dictionary<string, object> Arguments { get; set; } = new();
}