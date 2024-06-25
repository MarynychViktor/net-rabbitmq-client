using AMQPClient.Protocol.Attributes;

namespace AMQPClient.Protocol.Methods.Connection;

[MethodDef(classId: 10, methodId: 10)]
public class StartMethod : Method
{
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
}