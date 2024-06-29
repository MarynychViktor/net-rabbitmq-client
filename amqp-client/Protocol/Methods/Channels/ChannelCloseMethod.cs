using AMQPClient.Protocol.Attributes;

namespace AMQPClient.Protocol.Methods.Channels;

[MethodDef(20, 40)]
public class ChannelCloseMethod : Method
{
    [ShortField(0)] public short ReplyCode { get; set; } = 320;

    [ShortStringField(1)] public string ReplyText { get; set; } = "Closed by peer";

    [ShortField(2)] public short ReasonClassId { get; set; } = 0;

    [ShortField(3)] public short ReasonMethodId { get; set; } = 0;
}