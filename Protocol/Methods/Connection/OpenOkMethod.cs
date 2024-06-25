using AMQPClient.Protocol.Attributes;

namespace AMQPClient.Protocol.Methods.Connection;

[MethodDef(classId: 10, methodId: 41)]
public class OpenOkMethod : Method
{
    [ShortStringField(0)]
    public string Reserved1 { get; set; } = "";
}