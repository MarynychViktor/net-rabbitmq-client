using AMQPClient.Protocol.Attributes;

namespace AMQPClient.Protocol.Methods.Exchanges;

[MethodDef(40, 21)]
public class ExchangeDeleteOk : Method
{
    public override bool IsAsyncResponse() => true;
}