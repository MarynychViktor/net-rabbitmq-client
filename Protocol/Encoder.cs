using System.Reflection;
using AMQPClient.Protocol.Attributes;
using AMQPClient.Protocol.Methods;

namespace AMQPClient.Protocol;

public class Encoder
{
    public static byte[] MarshalMethodFrame<T>(T methodFrame) where T : Method
    {
        var writer = new BinWriter();
        var methodAttribute = (MethodDefAttribute)methodFrame.GetType().GetCustomAttribute(typeof(MethodDefAttribute))!;
        // Console.WriteLine($"Marshal method {methodFrame.GetType()}, CID {methodAttribute.ClassId} MID {methodAttribute.MethodId}");
        writer.Write(methodAttribute.ClassId);
        writer.Write(methodAttribute.MethodId);
        // writer.Write(Encoding.ASCII.GetBytes("AMQP"));
        // writer.Write(new byte[] { 0, 0, 9, 1 });


        var propertiesWithAttrs = methodFrame.GetType().GetProperties()
            .Select(info =>
            {
                var attrs = (MethodField[])info.GetCustomAttributes(typeof(MethodField), false);

                return (Property: info, FieldAttribute: attrs.Length > 0 ? attrs[0] : null);
            })
            .Where(data => data.FieldAttribute != null)
            .OrderBy(data => data.FieldAttribute.index);

        foreach (var (property, attribute) in propertiesWithAttrs)
            switch (attribute)
            {
                case ByteField attr:
                    // Console.WriteLine($"Write attribute {property.Name} {(byte)property.GetValue(methodFrame)}");
                    writer.Write((byte)property.GetValue(methodFrame));
                    break;
                case ShortField attr:
                    // Console.WriteLine($"Write attribute {property.Name} {(short)property.GetValue(methodFrame)}");
                    writer.Write((short)property.GetValue(methodFrame));
                    break;
                case IntField attr:
                    // Console.WriteLine($"Write attribute {property.Name} {(int)property.GetValue(methodFrame)}");
                    writer.Write((int)property.GetValue(methodFrame));
                    break;
                case LongField attr:
                    // Console.WriteLine($"Write attribute {property.Name} {(int)property.GetValue(methodFrame)}");
                    writer.Write((long)property.GetValue(methodFrame));
                    break;
                case PropertiesTableField attr:
                    // Console.WriteLine($"Write attribute {property.Name} {(Dictionary<string, object>)property.GetValue(methodFrame)}");
                    writer.WriteFieldTable((Dictionary<string, object>)property.GetValue(methodFrame));
                    break;
                case ShortStringField attr:
                    // Console.WriteLine($"Write attribute {property.Name} {(string)property.GetValue(methodFrame)}");
                    writer.WriteShortStr((string)property.GetValue(methodFrame));
                    break;
                case LongStringField attr:
                    // Console.WriteLine($"Write attribute {property.Name} {(string)property.GetValue(methodFrame)}");
                    writer.WriteLongStr((string)property.GetValue(methodFrame));
                    break;
                default:
                    throw new Exception($"Unrecognized {attribute.GetType().Name}");
            }

        return writer.ToArray();
    }
}