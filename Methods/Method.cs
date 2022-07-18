using System.Text;

namespace AMQPClient.Methods;

public interface Method
{
    public short ClassId { get; }
    public short MethodId { get; }
}