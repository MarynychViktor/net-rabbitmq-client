using AMQPClient.Protocol.Attributes;

namespace AMQPClient.Protocol.Methods.Basic;

[MethodDef(60, 110)]
public class BasicRecover : Method
{
    [ByteField(1)]
    public byte Requeue { get; set; }
}