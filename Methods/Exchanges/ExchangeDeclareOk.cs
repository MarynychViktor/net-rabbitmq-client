namespace AMQPClient.Methods.Exchanges;

public class ExchangeDeclareOk : Method
{
    public short ClassId { get; } = 40;
    public short MethodId { get; } = 11;
}