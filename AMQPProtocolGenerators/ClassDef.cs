namespace AMQPProtocolGenerators;

public class ClassDef
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<MethodDef> Methods { get; set; } = new();
}