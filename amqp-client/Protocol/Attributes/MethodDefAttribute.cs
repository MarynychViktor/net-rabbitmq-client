namespace AMQPClient.Protocol.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class MethodDefAttribute : Attribute
{
    public MethodDefAttribute(short classId, short methodId)
    {
        ClassId = classId;
        MethodId = methodId;
    }

    public short ClassId { get; }
    public short MethodId { get; }
}