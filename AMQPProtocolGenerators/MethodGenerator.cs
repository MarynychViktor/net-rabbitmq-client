using System.Text;

namespace AMQPProtocolGenerators;

public class MethodGenerator
{
    public static string GenerateDefinition(ClassDef klass, MethodDef method)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"\tpublic class {StrFormatUtils.ToPascalCase(method.Name)} : IFrameMethod {{");
        builder.AppendLine($"\t\tprivate const short _sourceClassId = {klass.Id};");
        builder.AppendLine($"\t\tprivate const short _sourceMethodId = {method.Id};");
        builder.AppendLine($"\t\tpublic short SourceClassId => _sourceClassId;");
        builder.AppendLine($"\t\t public short SourceMethodId => _sourceMethodId;");
        builder.AppendLine($"\t\tpublic const bool IsAsyncResponse = {(method.IsAsyncResponse ? "true" : "false")};");
        builder.AppendLine($"\t\tpublic const bool HasBody = false;");
        
        if (method.Fields.Any()) builder.AppendLine();

        foreach (var field in method.Fields)
        {
            var localDomain = DomainTypes.DomainToInternalTypeMap[field.Domain];
            builder.AppendLine($"\t\tpublic {(localDomain == "byte" ? "bool" : localDomain )} {StrFormatUtils.ToPascalCase(field.Name)} {{ get; set; }}");
        }

        GenerateSerializer(builder, method);
        builder.AppendLine();

        GenerateDeserializer(builder, method);
        builder.AppendLine("\t}");
    
        return builder.ToString();
    }

    private static void GenerateSerializer(StringBuilder builder, MethodDef method)
    {
        builder.AppendLine();
        builder.AppendLine("\t\tpublic byte[] Serialize() {");
        builder.AppendLine("\t\t\tvar writer = new BinWriter();");
        builder.AppendLine($"\t\t\twriter.WriteShort(SourceClassId);");
        builder.AppendLine($"\t\t\twriter.WriteShort(SourceMethodId);");

        FieldDef? prevField = null;
        for (var i = 0; i < method.Fields.Count; i++)
        {
            var field = method.Fields[i];
            var localDomain = DomainTypes.DomainToInternalTypeMap[field.Domain];
            if (localDomain == "byte")
            {
                var append = prevField != null && string.Equals(DomainTypes.DomainToInternalTypeMap[prevField.Domain], localDomain) ? "true" : "false";
                
                var j = 1;

                while (true)
                {
                    if (i >= method.Fields.Count) break;
                    
                    var bitField = method.Fields[i];
                    localDomain = DomainTypes.DomainToInternalTypeMap[bitField.Domain];
                    append = prevField != null && string.Equals(DomainTypes.DomainToInternalTypeMap[prevField.Domain], localDomain) ? "true" : "false";
                    if (localDomain != "byte")
                    {
                        i--;
                        break;
                    }

                    builder.AppendLine($"\t\t\twriter.{DomainTypes.GetDomainWriterMethod(field.Domain)}((byte)({StrFormatUtils.ToPascalCase(bitField.Name)} ? {j} : 0), {append});");
                    j <<= 1;
                    i++;
                    prevField = bitField;
                }
            }
            else
            {
                builder.AppendLine($"\t\t\twriter.{DomainTypes.GetDomainWriterMethod(field.Domain)}({StrFormatUtils.ToPascalCase(field.Name)});");
            }

            prevField = field;
        }

        builder.AppendLine("\t\t\treturn writer.ToArray();");
        builder.AppendLine("\t\t}");
    }

    private static void GenerateDeserializer(StringBuilder builder, MethodDef method)
    {
        builder.AppendLine("\t\tpublic void Deserialize(byte[] bytes) {");
        builder.AppendLine("\t\t\tvar reader = new BinReader(bytes);");
        builder.AppendLine($"\t\t\treader.ReadShort();");
        builder.AppendLine($"\t\t\treader.ReadShort();");

        FieldDef? prevField = null;
        for (var i = 0; i < method.Fields.Count; i++)
        {
            var field = method.Fields[i];
            var localDomain = DomainTypes.DomainToInternalTypeMap[field.Domain];

            if (localDomain == "byte")
            {
                builder.AppendLine($"\t\t\tvar flags = reader.ReadByte();");

                var j = 1;

                while (true)
                {
                    if (i >= method.Fields.Count) break;

                    var bitField = method.Fields[i];
                    localDomain = DomainTypes.DomainToInternalTypeMap[bitField.Domain];
                    if (localDomain != "byte")
                    {
                        i--;
                        break;
                    }

                    builder.AppendLine($"\t\t\t{StrFormatUtils.ToPascalCase(bitField.Name)} = (flags & {j}) > 0;");
                    j <<= 1;
                    i++;
                }
            }
            else
            {
                builder.AppendLine($"\t\t\t{StrFormatUtils.ToPascalCase(field.Name)} = reader.{DomainTypes.GetDomainReaderMethod(field.Domain)}();");
            }
        }

        builder.AppendLine("\t\t}");
    }
}