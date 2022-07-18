using System.Text;

namespace AMQPClient.Methods.Connection;

public class ConnectionClose : Method
{
    [ShortField(0)] public short ClassId { get; set; } = 10;

    [ShortField(1)] public short MethodId { get; set; } = 50;

    [ShortField(2)]
    public short ReplyCode { get; set; }

    [ShortStringField(3)]
    public string ReplyText { get; set; }


    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.AppendLine($"ClassId: {ClassId}");
        builder.AppendLine($"MethodId: {MethodId}");
        builder.AppendLine($"ReplyCode: {ReplyCode}");
        builder.AppendLine($"ReplyText: {ReplyText}");
       
        return builder.ToString();
    }
}