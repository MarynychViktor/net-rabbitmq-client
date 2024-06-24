namespace AMQPClient.Protocol;

public class Decoder
{
    public static T CreateMethodFrame<T>(byte[] body) where T : class, new()
    {
        var reader = new BinReader(body);
        // Skip classId
        reader.ReadInt16();
        // Skip methodId
        reader.ReadInt16();
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
                case ByteField:
                    property.SetValue(method, reader.ReadByte());
                    break;
                case ShortField:
                    property.SetValue(method, reader.ReadInt16());
                    break;
                case IntField:
                    property.SetValue(method, reader.ReadInt32());
                    break;
                case LongField:
                    property.SetValue(method, reader.ReadInt64());
                    break;
                case PropertiesTableField:
                    property.SetValue(method, reader.ReadFieldTable());
                    break;
                case LongStringField:
                    property.SetValue(method, reader.ReadLongStr());
                    break;
                case ShortStringField:
                    property.SetValue(method, reader.ReadShortStr());
                    break;
                default:
                    throw new Exception($"Please add support for field type: {attribute}");
            }
                            
        }

        return method;
    }
}