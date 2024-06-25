using AMQPClient.Protocol.Attributes;

namespace AMQPClient.Protocol.Methods.Connection;

[MethodDef(classId: 10, methodId: 11)]
public class StartOkMethod : Method
{
    [PropertiesTableField(0)]
    public Dictionary<string, object> ClientProperties { get; set; }

    [ShortStringField(1)]
    public string Mechanism { get; set; }
    
    [LongStringField(2)]
    public string Response { get; set; }

    [ShortStringField(3)]
    public string Locale { get; set; }
}