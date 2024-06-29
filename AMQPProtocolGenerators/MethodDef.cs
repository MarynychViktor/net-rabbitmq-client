namespace AMQPProtocolGenerators;

public class MethodDef
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsAsyncResponse { get; set; }
    public bool HasBody { get; set; }
    public List<FieldDef> Fields { get; set; } = new();
}