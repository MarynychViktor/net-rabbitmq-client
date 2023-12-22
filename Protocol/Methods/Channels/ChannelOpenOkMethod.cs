namespace AMQPClient.Methods.Channels;

[MethodDef(classId: 20, methodId: 11)]
public class ChannelOpenOkMethod : Method
{
    [ShortStringField(0)]
    private string Reserved1 { get; set; } = "";
}