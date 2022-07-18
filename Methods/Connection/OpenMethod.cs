using System.Text;

namespace AMQPClient.Methods.Connection;

public class OpenMethod : Method
{
    public short ClassId => 10;
    public short MethodId => 40;

    [ShortStringField(0)]
    public string VirtualHost { get; set; }

    [ShortStringField(1)] public string Reserved1 { get; set; } = "";

    [ByteField(2)] public byte Reserved2 { get; set; } = 0;

    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.AppendLine($"VirtualHost: {VirtualHost}");
       
        return builder.ToString();
    }
}