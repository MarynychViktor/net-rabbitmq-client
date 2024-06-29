using AMQPClient.Protocol.Attributes;

namespace AMQPClient.Protocol.Methods.Basic;

[MethodDef(60, 10)]
public class BasicQos: Method
{    
    [IntField(0)]
    public int PrefetchSize { get; set; }

    [ShortField(1)]
    public short PrefetchCount { get; set; }

    [ByteField(2)]
    public byte Global { get; set; }
}