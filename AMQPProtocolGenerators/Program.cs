// See https://aka.ms/new-console-template for more information

using System.Text;
using System.Xml;
using AMQPProtocolGenerators;

Console.WriteLine("Hello, World!");
var xmlDocument = new XmlDocument();
xmlDocument.Load("amqp0-9-1.xml");

var root = xmlDocument.DocumentElement;

if (root == null) throw new ArgumentException();

var classes = new List<ClassDef>();
var asyncResponsesMap = new Dictionary<string, bool>();

var domainNodes = root.SelectNodes("domain");
// var sb = new StringBuilder();
// sb.AppendLine("public static Dictionary<string, string> DomainToInternalTypeMap = new() {");
// foreach (XmlElement domainNode in domainNodes)
// {
//     sb.AppendLine($"{{ \"{domainNode.Attributes["name"].Value}\", \"{domainNode.Attributes["type"].Value}\" }},");
// }
// sb.AppendLine("}");
// Console.WriteLine(sb);
// return;
foreach (var node in root)
{
    if (node is XmlElement classNode)
    {
        if (classNode.Name == "class")
        {
            var className = classNode.Attributes["name"]!.Value;
            var classId = classNode.Attributes["index"]!.Value;

            // if (className != "queue") continue;

            var klass = new ClassDef()
            {
                Id = short.Parse(classId),
                Name = className,
            };
            classes.Add(klass);

            Console.WriteLine($"Class {className} id {classId}");

            foreach (var subClassNode in classNode)
            {
                if (subClassNode is XmlElement methodNode)
                {
                    if (methodNode.Name == "method")
                    {
                        var methodId = methodNode.Attributes["index"]!.Value;
                        var methodName = methodNode.Attributes["name"]!.Value;
                    
                        // if (methodName != "declare") continue;

                        Console.WriteLine($"\tMethod {methodName}");

                        var response = methodNode.SelectSingleNode("response");
                        if (response != null)
                        {
                            Console.WriteLine($"\tResponse {response.Attributes["name"].Value}");
                            asyncResponsesMap[response.Attributes["name"]!.Value] = true;
                            asyncResponsesMap[methodName] = false;
                        }

                        var method = new MethodDef()
                        {
                            Id = short.Parse(methodId),
                            Name =  methodName,
                            IsAsyncResponse = asyncResponsesMap.ContainsKey(methodName) ? asyncResponsesMap[methodName] : false
                        };
                        klass.Methods.Add(method);

                        var fields = methodNode.SelectNodes("field");

                        foreach (var attrNode in fields)
                        {
                            if (attrNode is not XmlElement fieldNode) continue;

                            Console.WriteLine($"\t\tField {fieldNode.Attributes["name"].Value}");

                            var fieldDef = new FieldDef()
                            {
                                Name = fieldNode.Attributes["name"].Value,
                                Domain = fieldNode.Attributes["domain"] != null ? fieldNode.Attributes["domain"].Value : fieldNode.Attributes["type"].Value,
                                IsReserved = fieldNode.HasAttribute("reserved")
                            };
                            method.Fields.Add(fieldDef);
                        }
                    }  
                }
            }
        }
    }
}


// var classBuilder = new StringBuilder();


void GenerateDeserializer(StringBuilder classBuilder, ClassDef klass, MethodDef method)
{
    classBuilder.AppendLine("\tpublic void Deserialize(byte[] bytes) {");
    classBuilder.AppendLine("\t\tvar reader = new BitReader(bytes);");
    classBuilder.AppendLine($"\t\treader.ReadShort(ClassId);");
    classBuilder.AppendLine($"\t\treader.ReadShort(MethodId);");

    FieldDef? prevField = null;
    for (var i = 0; i < method.Fields.Count; i++)
    {
        var field = method.Fields[i];
        if (field.IsReserved)
        {
            continue;
        }

        var localDomain = DomainTypes.GetDomainWriterMethod(field.Domain);
        if (localDomain == "bit")
        {
            classBuilder.AppendLine($"\t\tvar flags = reader.Read{StrFormatUtils.ToPascalCase(field.Domain)}();");

            var j = 1;

            while (true)
            {
                if (i >= method.Fields.Count) break;

                var bitField = method.Fields[i];
                localDomain = DomainTypes.GetDomainWriterMethod(bitField.Domain);
                if (localDomain != "bit")
                {
                    i--;
                    break;
                }

                classBuilder.AppendLine($"\t\t{StrFormatUtils.ToPascalCase(bitField.Name)} = flags & {j} > 0;");
                j <<= 1;
                i++;
            }
        }
        else
        {
            classBuilder.AppendLine($"\t\t{StrFormatUtils.ToPascalCase(field.Name)} = reader.Read{DomainTypes.GetDomainWriterMethod(field.Domain)}();");
        }

        prevField = field;
    }

    classBuilder.AppendLine("\t}");
}

foreach (var klass in classes)
{
    var classBuilder = new StringBuilder();
    var builder = new StringBuilder();
    builder.AppendLine("using AMQPClient.Protocol;");
    builder.AppendLine();
    builder.AppendLine($"public class {StrFormatUtils.ToPascalCase(klass.Name)} {{");
    foreach (var method in klass.Methods)
    {


        var definition = MethodGenerator.GenerateDefinition(klass, method);


        // Console.Write(builder);
        // return;
        // var file = File.OpenWrite(args[0] + "/" + DomainUtils.ToPascalCase($"{klass.Name}-{method.Name}") + ".cs");

        builder.Append(definition);
        builder.AppendLine();

        // Console.WriteLine(MethodGenerator.GenerateDefinition(klass, method));
        continue;
        classBuilder.AppendLine($"public record {StrFormatUtils.ToPascalCase($"{klass.Name}-{method.Name}")} {{");
        classBuilder.AppendLine($"\tpublic const short ClassId = {klass.Id};");
        classBuilder.AppendLine($"\tpublic const short MethodId = {method.Id};");
        classBuilder.AppendLine($"\tpublic const bool IsAsyncResponse = {method.IsAsyncResponse};");
        classBuilder.AppendLine($"\tpublic const bool HasBody = false;");
        classBuilder.AppendLine();

        foreach (var field in method.Fields)
        {
            var localDomain = DomainTypes.DomainToInternalTypeMap[field.Domain];
            classBuilder.AppendLine($"\tpublic {localDomain} {StrFormatUtils.ToPascalCase(field.Name)} {{get;set;}}");
        }
        classBuilder.AppendLine();

        classBuilder.AppendLine("\tpublic void Serialize() {");
        classBuilder.AppendLine("\t\tvar writer = new BinWriter();");
        FieldDef? prevField = null;
        classBuilder.AppendLine($"\t\twriter.write(ClassId);");
        classBuilder.AppendLine($"\t\twriter.write(MethodId);");
        foreach (var field in method.Fields)
        {
            var localDomain = DomainTypes.DomainToInternalTypeMap[field.Domain];
            if (localDomain == "byte")
            {
                var append = prevField != null && !string.Equals(prevField.Domain, field.Domain) ? "true" : "false";
                classBuilder.AppendLine($"\t\twriter.write{StrFormatUtils.ToPascalCase(field.Domain)}({StrFormatUtils.ToPascalCase(field.Name)}, {append});");
            }
            else
            {
                classBuilder.AppendLine($"\t\twriter.write{DomainTypes.GetDomainWriterMethod(field.Domain)}({StrFormatUtils.ToPascalCase(field.Name)});");
            }

            prevField = field;
        }

        classBuilder.AppendLine("\t\treturn writer.ToArray();");
        classBuilder.AppendLine("\t}");
        classBuilder.AppendLine();

        GenerateDeserializer(classBuilder, klass, method);

        classBuilder.AppendLine($"}}");
    }
    // builder.Append(b);
    builder.AppendLine("}");

    File.WriteAllText(args[0] + "/" + StrFormatUtils.ToPascalCase($"{klass.Name}") + ".cs", builder.ToString());

    // Console.WriteLine(classBuilder.ToString());
}