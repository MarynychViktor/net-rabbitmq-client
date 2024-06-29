using AMQPClient.Protocol.Attributes;

namespace AMQPClient.Protocol.Methods.Channels;

[MethodDef(20, 21)]
public class ChannelFlowOkMethod : Method
{
    [ByteField(0)]
    public byte Active { get; set; } = 0;
}