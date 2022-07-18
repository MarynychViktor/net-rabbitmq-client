using System.Text;
using AMQPClient.Methods;

namespace AMQPClient.Protocol;

public class Encoder
{
    public static byte[] MarshalMethodFrame<T>(T methodFrame) where T : Method, new()
    {

        var writer = new BinWriter();
        writer.Write(methodFrame.ClassId);
        writer.Write(methodFrame.MethodId);
        // writer.Write(Encoding.ASCII.GetBytes("AMQP"));
        // writer.Write(new byte[] { 0, 0, 9, 1 });
        

        var propertiesWithAttrs = typeof(T).GetProperties()
            .Select(info =>
            {
                var attrs = (MethodField[])info.GetCustomAttributes(typeof(MethodField), false);

                return (Property: info, FieldAttribute: attrs.Length > 0 ? attrs[0] : null);
            })
            .Where(data => data.FieldAttribute != null)
            .OrderBy(data => data.FieldAttribute.index);
   
        foreach (var (property, attribute) in propertiesWithAttrs)
        {
            switch (attribute)
            {
                case ByteField attr:
                    writer.Write((byte)property.GetValue(methodFrame));
                    break;
                case ShortField attr:
                    writer.Write((short)property.GetValue(methodFrame));
                    break;
                case IntField attr:
                    writer.Write((int)property.GetValue(methodFrame));
                    break;
                case PropertiesTableField attr:
                    writer.WriteFieldTable((Dictionary<string, object>)property.GetValue(methodFrame));
                    break;
                case ShortStringField attr:
                    writer.WriteShortStr((string)property.GetValue(methodFrame));
                    break;
                case LongStringField attr:
                    writer.WriteLongStr((string)property.GetValue(methodFrame));
                    break;
                default:
                    throw new Exception($"Unrecognized {attribute.GetType().Name}");
            }
        }

        return writer.ToArray();
    }
}