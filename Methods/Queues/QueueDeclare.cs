namespace AMQPClient.Methods.Exchanges;
// reserved1: Short, name: ShortStr, ty: ShortStr, flags: Byte, props: PropTable
[MethodDef(classId: 50, methodId: 10)]
public class QueueDeclare : Method
{
    [ShortField(0)]
    public short Reserved1 { get; set; }

    [ShortStringField(1)]
    public string Name { get; set; }

    [ByteField(2)]
    public byte Flags { get; set; } = 0;

    [PropertiesTableField(3)]
    public Dictionary<string, object> Arguments { get; set; } = new ();
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
}