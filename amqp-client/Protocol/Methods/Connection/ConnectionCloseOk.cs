using AMQPClient.Protocol.Attributes;

namespace AMQPClient.Protocol.Methods.Connection;

[MethodDef(10, 51)]
public class ConnectionCloseOk : Method
{
    public override bool IsAsyncResponse() => true;
}