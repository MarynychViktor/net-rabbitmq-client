using System.Text;

namespace AMQPClient.Methods.Connection;

public class StartOkMethod : Method
{
    public short ClassId => 10;
    public short MethodId => 11;

    [PropertiesTableField(0)]
    public Dictionary<string, object> ClientProperties { get; set; }

    [ShortStringField(1)]
    public string Mechanism { get; set; }
    
    [LongStringField(2)]
    public string Response { get; set; }

    [ShortStringField(3)]
    public string Locale { get; set; }


    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.AppendLine("ClientProperties:");
        foreach (var (k, v) in ClientProperties)
        {
            builder.AppendLine($"\t{k}: {v}");
        }

        builder.AppendLine($"Mechanism: {Mechanism}");
        builder.AppendLine($"Response: {Response}");
        builder.AppendLine($"Locales: {Locale}");

        return builder.ToString();
    }
}