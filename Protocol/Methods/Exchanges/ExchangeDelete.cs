using AMQPClient.Protocol.Attributes;

namespace AMQPClient.Protocol.Methods.Exchanges;

[MethodDef(classId: 40, methodId: 20)]
public class ExchangeDelete : Method
{
    [ShortField(0)] public short Reserved1 { get; set; }

    [ShortStringField(1)]
    public string Name { get; set; }

    // FIXME: byte with multiple props
    [ByteField(2)]
    public byte IfUnused { get; set; }
    //
    // [ByteField(3)]
    // public byte NoWait { get; set; }
}