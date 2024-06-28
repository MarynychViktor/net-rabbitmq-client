namespace AMQPClient;

public class ResourceClosedException : Exception
{
    public short? ReasonCode { get; }
    public string? ReasonText { get; }

    public ResourceClosedException(short? reasonCode, string? reasonText) : base($"Resource was closed with {reasonCode} {reasonText}")
    {
        ReasonCode = reasonCode;
        ReasonText = reasonText;
    }

    public ResourceClosedException() : base("Attempt to invoke method on closed resource")
    {
    }
}