using AMQPClient.Protocol.Attributes;

namespace AMQPClient.Protocol.Methods.Connection;

[MethodDef(10, 50)]
public class ConnectionClose : Method
{
    [ShortField(2)] public short ReplyCode { get; set; }

    [ShortStringField(3)] public string ReplyText { get; set; }
}