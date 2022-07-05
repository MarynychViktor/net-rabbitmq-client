using AMQPClient.Protocol;

namespace AMQPClient;

public class DefaultAmqpChannel : IAmqpChannel
{
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