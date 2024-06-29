using AMQPClient.Protocol.Attributes;

namespace AMQPClient.Protocol.Methods.Channels;

[MethodDef(20, 20)]
public class ChannelFlowMethod : Method
{
    [ByteField(0)]
    public byte Active { get; set; } = 0;
}