using System.Reflection;
using System.Text;

namespace AMQPClient.Methods;

public class Method
{
    public (short, short) ClassMethodId()
    {
        var attr = (MethodDefAttribute) GetType().GetCustomAttribute(typeof(MethodDefAttribute))!;
        return (attr.ClassId, attr.MethodId);
    }

    public override string ToString()
    {
        var stringBuilder = new StringBuilder();

        foreach (var propertyInfo in GetType().GetProperties())
        {
            if (propertyInfo.PropertyType == typeof(Dictionary<,>))
            {
                stringBuilder.AppendLine($"{propertyInfo.Name}");
                var property = (Dictionary<string, object>)propertyInfo.GetValue(this)!;

                foreach (var (k, v) in property)
                {
                    stringBuilder.AppendLine($"\t{k}: {v}");
                }
            }
            else
            {
                stringBuilder.Append($"{propertyInfo.Name}: {propertyInfo.GetValue(this)}");
            }
        }

        return stringBuilder.ToString();
    }
}