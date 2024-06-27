using AMQPClient.Protocol.Attributes;

namespace AMQPClient.Protocol.Methods.Basic;

[MethodDef(60, 90)]
public class BasicReject : Method
{
    [LongField(0)]
    public long Tag { get; set; }

    [ByteField(1)]
    public byte Requeue { get; set; }
}