using System.Xml;
using AMQPProtocolGenerators;

var xmlDocument = new XmlDocument();
xmlDocument.Load("amqp0-9-1.xml");

var root = xmlDocument.DocumentElement;
if (root == null) throw new ArgumentException();

var classes = new List<ClassDef>();
var asyncResponsesMap = new Dictionary<string, bool>();

foreach (var node in root)
{
    if (node is XmlElement classNode)
    {
        if (classNode.Name == "class")
        {
            var className = classNode.Attributes["name"]!.Value;
            var classId = classNode.Attributes["index"]!.Value;

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

var version = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

foreach (var klass in classes)
{
    ClassGenerator.GenerateDefinition(klass, args[0], version);
}