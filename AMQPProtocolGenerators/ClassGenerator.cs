using System.Text;

namespace AMQPProtocolGenerators;

public static class ClassGenerator
{
    public static void GenerateDefinition(ClassDef klass, string path)
    {
        var builder = new StringBuilder();
        builder.AppendLine("namespace AMQPClient.Protocol.Classes;");
        builder.AppendLine();
        builder.AppendLine($"public static class {StrFormatUtils.ToPascalCase(klass.Name)} {{");

        foreach (var method in klass.Methods)
        {
            var definition = MethodGenerator.GenerateDefinition(klass, method);
            builder.Append(definition);
            builder.AppendLine();
        }

        builder.AppendLine("}");

        File.WriteAllText(path + "/" + StrFormatUtils.ToPascalCase($"{klass.Name}") + ".cs", builder.ToString());
    }
}