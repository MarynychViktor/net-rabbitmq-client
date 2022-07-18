namespace AMQPClient.Protocol;

public class Decoder
{
    public static T UnmarshalMethodFrame<T>(byte[] body) where T : class, new()
    {
        var reader = new BinReader(body);
        // Skip classId
        var classId = reader.ReadInt16();
        // Skip methodId
        var methodId = reader.ReadInt16();

        var method = new T();

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
                    property.SetValue(method, reader.ReadByte());
                    break;
                case ShortField attr:
                    property.SetValue(method, reader.ReadInt16());
                    break;
                case IntField attr:
                    property.SetValue(method, reader.ReadInt32());
                    break;
                case LongField attr:
                    property.SetValue(method, reader.ReadInt64());
                    break;
                case PropertiesTableField attr:
                    property.SetValue(method, reader.ReadFieldTable());
                    break;
                case LongStringField attr:
                    property.SetValue(method, reader.ReadLongStr());
                    break;
                case ShortStringField attr:
                    property.SetValue(method, reader.ReadShortStr());
                    break;
                default:
                    throw new Exception($"Please add support for field type: {attribute}");
            }
                            
        }

        return method;
    }
}