namespace AMQPClient.Protocol;

public interface IFrameMethod
{
    public short SourceClassId { get; }
    public short SourceMethodId { get; }
    public const bool IsAsyncResponse = true;
    public const bool HasBody = false;
    public byte[] Serialize();
    public void Deserialize(byte[] bytes);
}