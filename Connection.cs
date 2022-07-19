using System.Collections.Concurrent;
using System.Net.Sockets;
using AMQPClient.Protocol;
using Chan = System.Threading.Channels;

namespace AMQPClient;

public class Connection
{
    private TcpClient _client;
    private uint HeaderSize = 7;
    private Dictionary<int, IAmqpChannel> _channels = new();
    private int _channelId;
    private BlockingCollection<object> queue = new ();
    private DefaultAmqpChannel _defaultChannel => (DefaultAmqpChannel)_channels[0];

    public Connection()
    {
        _channels.Add(0, new DefaultAmqpChannel(this));
    }

    private short NextChannelId()
    {
        Interlocked.Increment(ref _channelId);
        return (short)_channelId;
    }
    
    public async Task<Channel> CreateChannelAsync()
    {
        var channel = new Channel(this, NextChannelId());
        _channels.Add(_channelId, channel);
        await channel.OpenAsync();

        return channel;
    }

    public async Task OpenAsync()
    {
        SetupTcpClient();
        StartListener();
        _defaultChannel.SendProtocolHeader();

        queue.Take();
    }

    private void SetupTcpClient()
    {
        _client = new TcpClient("localhost", 5672);
    }

    private void StartListener()
    {
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
        }, TaskCreationOptions.LongRunning);
    }
    
    internal void OpenEnd()
    {
        queue.Add(null);
        // _openChannel.Writer.WriteAsync(0);
    }

    internal void SendFrame(AMQPFrame frame)
    {
        _client.Client.Send(frame.ToBytes());
    }

    internal void Send(byte[] bytes)
    {
        _client.Client.Send(bytes);
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