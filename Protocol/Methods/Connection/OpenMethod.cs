using AMQPClient.Protocol.Attributes;

namespace AMQPClient.Protocol.Methods.Connection;

[MethodDef(classId: 10, methodId: 40)]
public class OpenMethod : Method
{
    [ShortStringField(0)]
    public string VirtualHost { get; set; }

    [ShortStringField(1)]
    public string Reserved1 { get; set; } = "";

    [ByteField(2)]
    public byte Reserved2 { get; set; } = 0;
}