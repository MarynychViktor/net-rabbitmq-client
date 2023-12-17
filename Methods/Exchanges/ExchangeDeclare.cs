namespace AMQPClient.Methods.Exchanges;
// reserved1: Short, name: ShortStr, ty: ShortStr, flags: Byte, props: PropTable
[MethodDef(classId: 40, methodId: 10)]
public class ExchangeDeclare : Method
{
    [ShortField(0)]
    public short Reserved1 { get; set; }

    [ShortStringField(1)]
    public string Name { get; set; }

    [ShortStringField(2)]
    public string Type { get; set; } = "direct";

    [ByteField(3)]
    public byte Flags { get; set; } = 0;
    //
    // [ByteField(4)]
    // public byte Durable { get; set; } = 0;
    //      
    // [ByteField(5)]
    // public byte AutoDelete { get; set; } = 0;
    //          
    // [ByteField(6)]
    // public byte Internal { get; set; } = 0;
    //
    // [ByteField(7)]
    // public byte NoWait { get; set; } = 0;

    [PropertiesTableField(8)]
    public Dictionary<string, object> Arguments { get; set; } = new ();
}