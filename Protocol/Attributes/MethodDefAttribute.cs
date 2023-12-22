namespace AMQPClient;

[AttributeUsage(AttributeTargets.Class)]
public class MethodDefAttribute : Attribute
{
    public short ClassId { get;  }
    public short MethodId { get; }

    public MethodDefAttribute(short classId, short methodId)
    {
        ClassId = classId;
        MethodId = methodId;
    }
}