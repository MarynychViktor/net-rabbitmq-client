using AMQPClient.Protocol.Attributes;

namespace AMQPClient.Protocol.Methods.Basic;

[MethodDef(classId: 60, methodId: 21)]
public class BasicConsumeOk : Method
{
    [ShortStringField(0)]
    public string Tag { get; set; }
}