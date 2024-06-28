using AMQPClient.Protocol.Attributes;

namespace AMQPClient.Protocol.Methods.Channels;

[MethodDef(20, 11)]
public class ChannelOpenOkMethod : Method
{
    [ShortStringField(0)] private string Reserved1 { get; set; } = "";

    public override bool IsAsyncResponse() => true;
}