using System.Text;

namespace AMQPClient.Methods.Connection;

public class StartMethod
{
    public short ClassId => 10;
    public short MethodId => 10;

    [ByteField(0)]
    public byte VerMajor { get; set; }
    
    [ByteField(1)]
    public byte VerMinor { get; set; }

    [PropertiesTableField(2)]
    public Dictionary<string, object> ServerProperties { get; set; }

    [LongStringField(3)]
    public string Mechanisms { get; set; }

    [LongStringField(4)]
    public string Locales { get; set; }


    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Version Major: {VerMajor}");
        builder.AppendLine($"Version Minor: {VerMinor}");
        builder.AppendLine("ServerProperties:");

        foreach (var (k, v) in ServerProperties)
        {
            builder.AppendLine($"\t{k}: {v}");
        }
        builder.AppendLine($"Mechanisms: {Mechanisms}");
        builder.AppendLine($"Locales: {Locales}");

        return builder.ToString();
    }
}