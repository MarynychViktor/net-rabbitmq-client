namespace AMQPClient.Methods.Channels;

public class OpenMethod : Method
{
    public short ClassId => 20;
    public short MethodId => 10;

    [ShortStringField(0)]
    private string Reserved1 { get; set; }
}