namespace AMQPClient;

[AttributeUsage(AttributeTargets.Property)]
public class MethodField : Attribute
{
    public byte index;

    public MethodField(byte index)
    {
        this.index = index;
    }
}