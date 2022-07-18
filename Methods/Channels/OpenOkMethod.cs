namespace AMQPClient.Methods.Channels;

public class OpenOkMethod : Method
{
    public short ClassId => 20;
    public short MethodId => 11;

    [LongStringField(0)]
    private string Reserved1 { get; set; }
}