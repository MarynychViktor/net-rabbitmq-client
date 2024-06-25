using System.Reflection;
using System.Text;
using AMQPClient.Protocol.Attributes;

namespace AMQPClient.Protocol.Methods;

public class Method
{
    public (short, short) ClassMethodId()
    {
        var attr = (MethodDefAttribute) GetType().GetCustomAttribute(typeof(MethodDefAttribute))!;
        return (attr.ClassId, attr.MethodId);
    }

    public short ClassId => ((MethodDefAttribute) GetType().GetCustomAttribute(typeof(MethodDefAttribute))!).ClassId;
    public short MethodId => ((MethodDefAttribute) GetType().GetCustomAttribute(typeof(MethodDefAttribute))!).MethodId;

    public bool IsAsyncResponse() => MethodMetaRegistry.IsAsyncResponse(ClassId, MethodId);
    public bool HasBody() => MethodMetaRegistry.HasBody(ClassId, MethodId);

    public override string ToString()
    {
        var stringBuilder = new StringBuilder();

        foreach (var propertyInfo in GetType().GetProperties())
        {
            if (propertyInfo.PropertyType == typeof(Dictionary<string, Object>))
            {
                FormatDictionary(propertyInfo.Name, (Dictionary<string, Object>)propertyInfo.GetValue(this), stringBuilder);
            }
            else
            {
                stringBuilder.AppendLine($"{propertyInfo.Name}: {propertyInfo.GetValue(this)}");
            }
        }

        return stringBuilder.ToString();
    }

    private void FormatDictionary(string name, Dictionary<string, Object> value, StringBuilder stringBuilder, int depth = 0) {
        stringBuilder.AppendLine($"{new string('\t', depth)}[Dict]{name}:");

        foreach (var (k, v) in value)
        {
            if (v.GetType() == typeof(Dictionary<string, Object>))
            {
                FormatDictionary(k, (Dictionary<string, Object>)v, stringBuilder, depth + 1);
            }
            else
            {
                stringBuilder.AppendLine($"{new string('\t', depth + 1)}{k}: {v}");
            }
        }
    }
}