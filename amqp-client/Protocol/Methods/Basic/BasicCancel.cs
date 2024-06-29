using AMQPClient.Protocol.Attributes;

namespace AMQPClient.Protocol.Methods.Basic;

[MethodDef(60, 30)]
public class BasicCancel : Method
{
    [ShortStringField(0)]
    public string ConsumerTag { get; set; }

    [ByteField(1)]
    public byte NoWait { get; set; }
}