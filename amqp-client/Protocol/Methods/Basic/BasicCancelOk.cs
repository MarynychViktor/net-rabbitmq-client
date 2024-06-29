using AMQPClient.Protocol.Attributes;

namespace AMQPClient.Protocol.Methods.Basic;

[MethodDef(60, 31)]
public class BasicCancelOk : Method
{
    [ShortStringField(0)]
    public string ConsumerTag { get; set; }

    public override bool IsAsyncResponse() => true;
}