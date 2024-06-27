using AMQPClient.Protocol.Methods;

namespace AMQPClient;

public record MethodResult(Method? Method)
{
    public short? ErrorCode { get; }
    public string? ErrorMessage { get; }
    public bool IsOk() => ErrorCode == null && ErrorMessage == null;

    public MethodResult(Method? method, short errorCode, string errorMessage) : this(method)
    {
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }
}
