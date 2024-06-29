using AMQPClient.Protocol.Attributes;

namespace AMQPClient.Protocol.Methods.Basic;

[MethodDef(60, 72)]
public class BasicGetEmpty: Method
{    
    [ShortStringField(0)]
    public string Reserved1 { get; set; }
    
    public override bool IsAsyncResponse() => true;
}