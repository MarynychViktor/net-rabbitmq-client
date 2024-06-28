using AMQPClient.Protocol.Attributes;

namespace AMQPClient.Protocol.Methods.Basic;

[MethodDef(60, 21)]
public class BasicConsumeOk : Method
{
    [ShortStringField(0)]
    public string Tag { get; set; }

    public override bool IsAsyncResponse() => true;
}