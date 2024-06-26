using AMQPClient.Protocol.Attributes;

namespace AMQPClient.Protocol.Methods.Channels;

[MethodDef(20, 10)]
public class ChannelOpenMethod : Method
{
    [ShortStringField(0)] public string Reserved1 { get; set; } = "";
}