using AMQPClient.Protocol.Attributes;

namespace AMQPClient.Protocol.Methods.Basic;

[MethodDef(60, 71)]
public class BasicGetOk: Method
{    

    [LongField(0)]
    public long DeliverTag { get; set; }

    [ByteField(1)]
    public byte Redelivered { get; set; }

    [ShortStringField(2)]
    public string Exchange { get; set; }

    [ShortStringField(3)]
    public string RoutingKey { get; set; }

    [IntField(4)]
    public int MessageCount { get; set; }
    
    public override bool IsAsyncResponse() => true;
    public override bool HasBody() => true;
}