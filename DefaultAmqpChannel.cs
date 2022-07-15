using System.Reflection;
using System.Reflection.Metadata;
using AMQPClient.Methods.Connection;
using AMQPClient.Protocol;

namespace AMQPClient;

public class DefaultAmqpChannel : IAmqpChannel
{
    private readonly Connection _connection;

    private readonly Dictionary<int, Type> _methodIdTypeMap = new ()
    {
        {1010, typeof(StartMethod)}
    };

    public DefaultAmqpChannel(Connection connection)
    {
        _connection = connection;
    }

    public T UnmarshalMethodFrame<T>(byte[] body) where T : class, new()
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
                case PropertiesTableField attr:
                    property.SetValue(method, reader.ReadFieldTable());
                    break;
                case LongStringField attr:
                    property.SetValue(method, reader.ReadLongStr());
                    break;
            }
                            
        }

        return method;
    }

    public Task HandleFrameAsync(byte type, byte[] body)
    {
        Console.WriteLine($"Handle frame async called {body.Length}");

        try
        {

            var reader = new BinReader(body);
            short classId = reader.ReadInt16();
            short methodId = reader.ReadInt16();
            Console.WriteLine($"Handling method {methodId}");
            var methodType = _methodIdTypeMap[classId * 100 + methodId];

            if (classId != 10)
            {
                Console.WriteLine($"Unsupported class with id {classId}");
                throw new Exception("unsupported class");
            }
            else
            {
                Console.WriteLine($"Handling method {methodId}");
                switch (methodId)
                {
                    case 10:
                        Console.WriteLine($"Received start method");
                        // typeof(StartMethod).GetProperties()
                        //     .Select(info => (MethodField[])info.GetCustomAttributes(typeof(MethodField), false))
                        //     .Where(attrs => attrs.Length > 0)
                        //     .Select(attrs => attrs[0])
                        //     .OrderBy(attr => attr.index)
                        // var startMethod = new StartMethod();
                        // TODO: call method dynamically
                        var m = GetType().GetMethod("UnmarshalMethodFrame");
                        var mm = m.MakeGenericMethod(methodType);
                        mm.Invoke(this, new []{body});
                        
                        
                        StartMethod startMethod = UnmarshalMethodFrame<StartMethod>(body);
                        //
                        // var propertiesWithAttrs = typeof(StartMethod).GetProperties()
                        //     .Select(info =>
                        //     {
                        //         var attrs = (MethodField[])info.GetCustomAttributes(typeof(MethodField), false);
                        //
                        //         return (Property: info, FieldAttribute: attrs.Length > 0 ? attrs[0] : null);
                        //     })
                        //     .Where(data => data.FieldAttribute != null)
                        //     .OrderBy(data => data.FieldAttribute.index);
                        //
                        // foreach (var (property, attribute) in propertiesWithAttrs)
                        // {
                        //     switch (attribute)
                        //     {
                        //         case ByteField attr:
                        //             property.SetValue(startMethod, reader.ReadByte());
                        //             break;
                        //         case PropertiesTableField attr:
                        //             property.SetValue(startMethod, reader.ReadFieldTable());
                        //             break;
                        //         case LongStringField attr:
                        //             property.SetValue(startMethod, reader.ReadLongStr());
                        //             break;
                        //     }
                        //     
                        // }
                        foreach (var pair in startMethod.ServerProperties)
                        {
                            Console.WriteLine($"Attributes properties {pair.Key}, Value {pair.Value}");
                        }

                        throw new Exception("reached place");
                        foreach (var propertyInfo in typeof(StartMethod).GetProperties())
                        {
                            var fields = (MethodField[])propertyInfo.GetCustomAttributes(typeof(MethodField), false);
                            if (fields.Length == 0)
                            {
                                continue;
                            }

                            var fieldAttribute = fields[0];
                            
                        }

                        var majVersion = reader.ReadByte();
                        var minVersion = reader.ReadByte();
                        var serverProperties = reader.ReadFieldTable();
                        var mechanisms = reader.ReadLongStr();
                        var locales = reader.ReadLongStr();

                        Console.WriteLine(
                            $"Start properties maj: {majVersion}, min: {minVersion}, props: {serverProperties}, mech: {mechanisms}, locale: {locales}");
                        foreach (var pair in serverProperties)
                        {
                            Console.WriteLine($"Key {pair.Key}, Value {pair.Value}");
                        }

                        var stream = new MemoryStream();
                        var methodWriter = new BinWriter(stream);
                        methodWriter.Write((short)10);
                        methodWriter.Write((short)11);
                        methodWriter.WriteFieldTable(new Dictionary<string, object>()
                        {
                            {"product",     "simpleapp"},
                            {"platform",    "Erlang/OTP 24.3.4"},
                            {"copyright",   "param-pam-pam"},
                            {"information", "Licensed under the MPL 2.0"},
                        });
                        methodWriter.WriteShortStr("PLAIN");
                        methodWriter.WriteLongStr("\x00user\x00password");
                        methodWriter.WriteShortStr("en_US");
                        var methodBytes = stream.ToArray();

                        var frameStream = new MemoryStream();
                        methodWriter = new BinWriter(frameStream);
                        methodWriter.Write((byte)1);
                        methodWriter.Write((short)0);
                        methodWriter.Write(methodBytes.Length);
                        methodWriter.Write(methodBytes);
                        methodWriter.Write((byte)206);
                        _connection._client.Client.Send(frameStream.ToArray());
                        Console.WriteLine($"Send response {methodBytes.Length}");

                        break;
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Exception {e}");
        }

        return Task.CompletedTask;
    }
    
      public Task HandleMethodFrame(byte[] body)
    {
        Console.WriteLine($"Handle frame async called {body.Length}");

        try
        {

            var reader = new BinReader(body);
            short classId = reader.ReadInt16();
            short methodId = reader.ReadInt16();
            Console.WriteLine($"Handling method {methodId}");
            var methodType = _methodIdTypeMap[classId * 100 + methodId];

            if (classId != 10)
            {
                Console.WriteLine($"Unsupported class with id {classId}");
                throw new Exception("unsupported class");
            }
            else
            {
                Console.WriteLine($"Handling method {methodId}");
                switch (methodId)
                {
                    case 10:

                        // TODO: call method dynamically
                        var m = GetType().GetMethod("UnmarshalMethodFrame");
                        var mm = m.MakeGenericMethod(methodType);
                        mm.Invoke(this, new []{body});
                        
                        
                        StartMethod startMethod = UnmarshalMethodFrame<StartMethod>(body);

                        
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Exception {e}");
        }

        return Task.CompletedTask;
    }
}