using AMQPClient.Protocol.Attributes;

namespace AMQPClient.Protocol.Methods.Basic;

[MethodDef(60, 40)]
public class BasicPublish : Method
{
    [ShortField(0)]
    public short Reserved1 { get; set; }

    [ShortStringField(1)]
    public string Exchange { get; set; }

    [ShortStringField(2)]
    public string RoutingKey { get; set; }

    [ByteField(3)]
    public byte Flags { get; set; }
}