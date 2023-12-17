namespace AMQPClient.Methods.Channels;

[MethodDef(classId: 20, methodId: 10)]
public class ChannelOpenMethod : Method
{
    [ShortStringField(0)]
    public string Reserved1 { get; set; } = "";
}