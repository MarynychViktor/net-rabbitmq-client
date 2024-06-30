namespace AMQPClient.Protocol;

public interface IFrameMethod
{
    public short SourceClassId { get; }
    public short SourceMethodId { get; }
    public bool IsAsyncResponse { get; }
    public bool HasBody { get; }
    public byte[] Serialize();
    public void Deserialize(byte[] bytes);
}