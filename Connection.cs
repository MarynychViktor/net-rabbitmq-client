using System.IO.Pipelines;
using System.Net.Sockets;
using System.Text;

namespace AMQPClient;

public class Connection
{
    private TcpClient _client;
    private uint HeaderSize = 7;

    public Connection()
    {
        open();
    }

    private async void open()
    {
        _client = new TcpClient("localhost", 5672);
        // []byte{'A', 'M', 'Q', 'P', 0, 0, 9, 1})
        var b = new ArraySegment<byte>(
                Encoding.ASCII.GetBytes("AMQP")
            )
            .Concat(new byte[] { 0, 0, 9, 1 }).ToArray();
        _client.Client.Send(b);

        // while (true)
        // {
        //
        //     var frameHeader = await ReadAsync(7);
        //     var frameSize = BitConverter.ToInt16(new ArraySegment<byte>(frameHeader, 3, 4));
        //     var frameBody = await ReadAsync(frameSize);
        //
        // }
        //
    }

    private async Task<byte[]> ReadAsync(int size)
    {
        byte[] result = new byte[size];
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