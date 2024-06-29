using System.Text;

namespace AMQPProtocolGenerators;

public class MethodGenerator
{
    public static string GenerateDefinition(ClassDef klass, MethodDef method)
    {
        var builder = new StringBuilder();
        builder.AppendLine("using AMQPClient.Protocol;");
        builder.AppendLine();
        builder.AppendLine($"public record {DomainUtils.ToPascalCase($"{klass.Name}-{method.Name}")} {{");
        builder.AppendLine($"\tpublic const short ClassId = {klass.Id};");
        builder.AppendLine($"\tpublic const short MethodId = {method.Id};");
        builder.AppendLine($"\tpublic const bool IsAsyncResponse = {(method.IsAsyncResponse ? "true" : "false")};");
        builder.AppendLine($"\tpublic const bool HasBody = false;");
        builder.AppendLine();

        foreach (var field in method.Fields)
        {
            var localDomain = DomainTypes.DomainToInternalTypeMap[field.Domain];
            builder.AppendLine($"\tpublic {localDomain} {DomainUtils.ToPascalCase(field.Name)} {{get;set;}}");
        }
        builder.AppendLine();
        GenerateSerializer(builder, method);
        builder.AppendLine();

        GenerateDeserializer(builder, method);
        builder.AppendLine("}}");
    
        return builder.ToString();
    }

    private static void GenerateSerializer(StringBuilder builder, MethodDef method)
    {
        
        builder.AppendLine("\tpublic void Serialize() {");
        builder.AppendLine("\t\tvar writer = new BinWriter();");
        FieldDef? prevField = null;
        builder.AppendLine($"\t\twriter.Write(ClassId);");
        builder.AppendLine($"\t\twriter.Write(MethodId);");
        foreach (var field in method.Fields)
        {
            var localDomain = DomainTypes.DomainToInternalTypeMap[field.Domain];
            if (localDomain == "byte")
            {
                var append = prevField != null && !string.Equals(prevField.Domain, field.Domain) ? "true" : "false";
                builder.AppendLine($"\t\twriter.Write{DomainUtils.ToPascalCase(field.Domain)}({DomainUtils.ToPascalCase(field.Name)}, {append});");
            }
            else
            {
                builder.AppendLine($"\t\twriter.Write{DomainTypes.GetDomainVisitor(field.Domain)}({DomainUtils.ToPascalCase(field.Name)});");
            }

            prevField = field;
        }

        builder.AppendLine("\t\treturn writer.ToArray();");
        builder.AppendLine("\t}");
    }

    private static void GenerateDeserializer(StringBuilder builder, MethodDef method)
    {
        builder.AppendLine("\tpublic void Deserialize(byte[] bytes) {");
        builder.AppendLine("\t\tvar reader = new BitReader(bytes);");
        builder.AppendLine($"\t\treader.ReadShort(ClassId);");
        builder.AppendLine($"\t\treader.ReadShort(MethodId);");

        FieldDef? prevField = null;
        for (var i = 0; i < method.Fields.Count; i++)
        {
            var field = method.Fields[i];
            if (field.IsReserved)
            {
                continue;
            }

            var localDomain = DomainTypes.GetDomainVisitor(field.Domain);
            if (localDomain == "Bit")
            {
                builder.AppendLine($"\t\tvar flags = reader.Read{DomainUtils.ToPascalCase(field.Domain)}();");

                var j = 1;

                while (true)
                {
                    if (i >= method.Fields.Count) break;

                    var bitField = method.Fields[i];
                    localDomain = DomainTypes.GetDomainVisitor(bitField.Domain);
                    if (localDomain != "Bit")
                    {
                        i--;
                        break;
                    }

                    builder.AppendLine($"\t\t{DomainUtils.ToPascalCase(bitField.Name)} = flags & {j} > 0;");
                    j <<= 1;
                    i++;
                }
            }
            else
            {
                builder.AppendLine($"\t\t{DomainUtils.ToPascalCase(field.Name)} = reader.Read{DomainTypes.GetDomainVisitor(field.Domain)}();");
            }
        }
        builder.AppendLine("\t}");
    }
}