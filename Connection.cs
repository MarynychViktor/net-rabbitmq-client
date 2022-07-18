using System.Net.Sockets;
using System.Text;
using AMQPClient.Methods.Channels;
using AMQPClient.Protocol;
using Encoder = AMQPClient.Protocol.Encoder;
using Chan = System.Threading.Channels;

namespace AMQPClient;

public class Connection
{
    private TcpClient _client;
    private uint HeaderSize = 7;
    private Dictionary<int, IAmqpChannel> _channels = new();
    private int _channelId;
    private Chan.Channel<byte> _openChannel;

    public Connection()
    {
        _channels.Add(0, new DefaultAmqpChannel(this));
    }

    public async Task<Channel> CreateChannelAsync()
    {
        Interlocked.Increment(ref _channelId);

        var ch = new Channel(this, (short)_channelId);
        _channels.Add(_channelId, ch);

        await ch.OpenAsync();

        return ch;
        // return new Channel(_client, (short) _channelId);
    }
    
    private void SendProtocolHeader()
    {
        _client.Client.Send(Encoding.ASCII.GetBytes("AMQP").Concat(new byte[] { 0, 0, 9, 1 }).ToArray());
    }
    
    public async Task OpenAsync()
    {
        _openChannel = Chan.Channel.CreateBounded<byte>(1);
        _client = new TcpClient("localhost", 5672);
        SendProtocolHeader();

        Task.Factory.StartNew(async () =>
        {
            while (true)
            {
                var frame = await ReadFrameAsync();
                var channel = _channels.ContainsKey(frame.Channel)
                    ? _channels[frame.Channel]
                    : throw new Exception($"Invalid channel: {frame.Channel}");

                switch (frame.Type)
                {
                    case AMQPFrameType.Method:
                        await channel.HandleMethodFrameAsync(frame.Body);
                        break;
                    default:
                        throw new Exception($"Not matched type {frame.Type}");

                }
            }
        });

        Console.WriteLine("Wait for openChannel");
        await _openChannel.Reader.ReadAsync();
        Console.WriteLine("OpenChannel completed");
    }

    internal void OpenEnd()
    {
        _openChannel.Writer.WriteAsync(0);
    }
    
    internal void SendFrame(AMQPFrame frame)
    {
        _client.Client.Send(frame.ToBytes());
    }
    
    private async Task<AMQPFrame> ReadFrameAsync()
    {
        var header = await ReadAsync(7);

        var reader = new BinReader(header);
        var type = reader.ReadByte();
        var channel = reader.ReadInt16();
        var size = reader.ReadInt32();

        var frameBody = await ReadAsync(size);
        // Read EOF frame
        var end = await ReadAsync(1);
        Console.WriteLine($"Read {end[0]}");

        if (type == 1)
        {
            return AMQPFrame.MethodFrame(channel, frameBody);
        }
        else
        {
            throw new Exception("failed");
        }
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