using System.Reflection;
using System.Text;
using AMQPClient.Protocol.Attributes;

namespace AMQPClient.Protocol.Methods;

public class Method
{
    public short ClassId => ((MethodDefAttribute)GetType().GetCustomAttribute(typeof(MethodDefAttribute))!).ClassId;
    public short MethodId => ((MethodDefAttribute)GetType().GetCustomAttribute(typeof(MethodDefAttribute))!).MethodId;

    public virtual bool IsAsyncResponse() => false;

    public virtual bool HasBody() => false;

    public override string ToString()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append($"{GetType().Name} {{");

        foreach (var propertyInfo in GetType().GetProperties())
            if (propertyInfo.PropertyType == typeof(Dictionary<string, object>))
                FormatDictionary(propertyInfo.Name, (Dictionary<string, object>)propertyInfo.GetValue(this),
                    stringBuilder);
            else
                stringBuilder.Append($"[{propertyInfo.Name}]: {propertyInfo.GetValue(this)}, ");
        stringBuilder.Append('}');

        return stringBuilder.ToString();
    }

    private void FormatDictionary(string name, Dictionary<string, object> value, StringBuilder stringBuilder,
        int depth = 0)
    {
        stringBuilder.AppendLine($"{new string('\t', depth)}[Dict]{name}:");

        foreach (var (k, v) in value)
            if (v.GetType() == typeof(Dictionary<string, object>))
                FormatDictionary(k, (Dictionary<string, object>)v, stringBuilder, depth + 1);
            else
                stringBuilder.Append($"{new string('\t', depth + 1)}[{k}]: {v},");
    }
}