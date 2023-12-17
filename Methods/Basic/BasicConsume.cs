namespace AMQPClient.Methods.Basic;

[MethodDef(classId: 60, methodId: 20)]
public class BasicConsume : Method
{
    [ShortField(0)]
    public short Reserved1 { get; set; }

    [ShortStringField(1)]
    public string Queue { get; set; }

    [ShortStringField(2)] public string Tag { get; set; } = "";

    [ByteField(3)]
    public byte Flags { get; set; }

    [PropertiesTableField(4)] public Dictionary<string, object> Properties { get; set; } = new();
}