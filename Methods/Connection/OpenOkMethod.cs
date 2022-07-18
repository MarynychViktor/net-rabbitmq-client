using System.Text;

namespace AMQPClient.Methods.Connection;

public class OpenOkMethod : Method
{
    public short ClassId => 10;
    public short MethodId => 41;

    [ShortStringField(0)] public string Reserved1 { get; set; } = "";

    public override string ToString()
    {
        var builder = new StringBuilder();
       
        return builder.ToString();
    }
}