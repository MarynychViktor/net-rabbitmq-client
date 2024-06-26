using AMQPClient.Protocol.Attributes;

namespace AMQPClient.Protocol.Methods.Exchanges;

[MethodDef(40, 10)]
public class ExchangeDeclare : Method
{
    [ShortField(0)] public short Reserved1 { get; set; }

    [ShortStringField(1)] public string Name { get; set; }

    [ShortStringField(2)] public string Type { get; set; } = "direct";

    [ByteField(3)] public byte Flags { get; set; } = 0;

    [PropertiesTableField(8)] public Dictionary<string, object> Arguments { get; set; } = new();
}

[Flags]
public enum ExchangeDeclareFlags
{
    None = 0,
    Passive = 1,
    Durable = 1 << 1,
    AutoDelete = 1 << 2,
    Internal = 1 << 3,
    NoWait = 1 << 4
}