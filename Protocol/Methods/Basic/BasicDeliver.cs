namespace AMQPClient.Methods.Basic;

[MethodDef(classId: 60, methodId: 60)]
public class BasicDeliver : Method
{
    [ShortStringField(0)]
    public string ConsumerTag { get; set; }

    [IntField(1)]
    public int DeliverTag { get; set; }

    [ByteField(2)]
    public byte Redelivered { get; set; }

    [ShortStringField(3)]
    public string Exchange { get; set; }
    
    [ShortStringField(4)]
    public string RoutingKey { get; set; }

}