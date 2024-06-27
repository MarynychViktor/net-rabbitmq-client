using AMQPClient.Protocol.Attributes;

namespace AMQPClient.Protocol.Methods.Basic;

[MethodDef(60, 80)]
public class BasicAck : Method
{
    [LongField(1)]
    public long Tag { get; set; }

    [ByteField(1)]
    public byte Multiple { get; set; }
}