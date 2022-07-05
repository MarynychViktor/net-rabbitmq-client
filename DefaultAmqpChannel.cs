using AMQPClient.Protocol;

namespace AMQPClient;

public class DefaultAmqpChannel : IAmqpChannel
{
    private readonly Connection _connection;

    public DefaultAmqpChannel(Connection connection)
    {
        _connection = connection;
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
}