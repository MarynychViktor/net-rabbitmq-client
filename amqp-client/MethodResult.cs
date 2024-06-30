using AMQPClient.Protocol;
using AMQPClient.Protocol.Methods;

namespace AMQPClient;

public record MethodResult(AmqpMethodFrame? MethodFrame)
{
    public short? ErrorCode { get; }
    public string? ErrorMessage { get; }
    public bool IsOk() => ErrorCode == null && ErrorMessage == null;
    public IFrameMethod? Method => MethodFrame?.Method;

    public MethodResult(AmqpMethodFrame? methodFrame, short errorCode, string errorMessage) : this(methodFrame)
    {
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }
}
