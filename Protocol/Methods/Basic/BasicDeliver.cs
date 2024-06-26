using AMQPClient.Protocol.Attributes;

namespace AMQPClient.Protocol.Methods.Basic;

[MethodDef(60, 60)]
public class BasicDeliver : Method
{
    [ShortStringField(0)] public string ConsumerTag { get; set; }

    [LongField(1)] public long DeliverTag { get; set; }

    [ByteField(2)] public byte Redelivered { get; set; }

    [ShortStringField(3)] public string Exchange { get; set; }

    [ShortStringField(4)] public string RoutingKey { get; set; }
}