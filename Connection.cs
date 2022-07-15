using System.ComponentModel;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Text;
using AMQPClient.Protocol;

namespace AMQPClient;

public class Connection
{
    public TcpClient _client;
    private uint HeaderSize = 7;
    private Dictionary<int, IAmqpChannel> _channels = new();

    public Connection()
    {
        open();
        _channels.Add(0, new DefaultAmqpChannel(this));
    }

    private async void open()
    {
        _client = new TcpClient("localhost", 5672);
        // []byte{'A', 'M', 'Q', 'P', 0, 0, 9, 1})
        // var b = new ArraySegment<byte>(
        //         Encoding.ASCII.GetBytes("AMQP")
        //     )
        //     .Concat(new byte[] { 0, 0, 9, 1 }).ToArray();
        // _client.Client.Send(b);
        
        //*************
        // var reader = new BinaryReader(new MemoryStream(b));
        //*************
        
        // var reader.ReadByte();
        var memStream = new MemoryStream();
        var writer = new BinaryWriter(memStream, Encoding.ASCII);
        writer.Write(Encoding.ASCII.GetBytes("AMQP"));
        writer.Write(new byte[] { 0, 0, 9, 1 });
        _client.Client.Send(memStream.ToArray());

        // new List<char>() { 'A', 'M', 'P', 'Q' }.Select((c => c.))

        await Task.Factory.StartNew(async () =>
        {
            while (true)
            {
                Console.WriteLine("Waiting for data");
                var frameHeader = await ReadAsync(7);

                var reader = new BinReader(new MemoryStream(frameHeader));
                var frameType = reader.ReadByte();
                var channelId = reader.ReadInt16();
                var frameSize = reader.ReadInt32();
                
                Console.WriteLine($"Rec {BitConverter.ToInt32(frameHeader.Skip(3).Take(4).Reverse().ToArray())}");
                Console.WriteLine($"Header {frameHeader} - {frameHeader.Length} Frame {frameType}, channel {channelId}, size {frameSize}");
                var frameBody = await ReadAsync(frameSize);
                // Read EOF frame
                await ReadAsync(1);
                Console.WriteLine($"Handle body #{frameBody[frameBody.Length - 1]}");

                var channel = _channels.ContainsKey(channelId)
                    ? _channels[channelId]
                    : throw new Exception($"Invalid channel: {channelId}");

                Console.WriteLine($"Handle fr");
                await channel.HandleFrameAsync(frameType, frameBody);
            }
        });
    }

    private async Task<byte[]> ReadAsync(int size)
    {
        byte[] result = {};
        int bytesRead = 0;

        while (bytesRead < size)
        {
            byte[] buffer = new byte[size];
            bytesRead += await _client.Client.ReceiveAsync(buffer, SocketFlags.None);
            result = result.Concat(buffer).ToArray();
        }

        return result;
    }
}