using System.Collections.Concurrent;
using System.Net.Sockets;
using AMQPClient.Methods;
using AMQPClient.Protocol;
using Chan = System.Threading.Channels;

namespace AMQPClient;

public interface IAMQPConnection
{
    Task InvokeAsync<T>(T method);
    Task<TResponse> InvokeAsync<TMethod, TResponse>(TMethod method);
}

public class Connection2
{
    private TcpClient _tcpClient;
    private uint HeaderSize = 7;
    private Dictionary<int, IAmqpChannel> _channels = new();
    private int _channelId;
    private BlockingCollection<object> queue = new ();
    private DefaultAmqpChannel _defaultChannel => (DefaultAmqpChannel)_channels[0];
    private AmqpStreamWrapper _amqpStreamWrapper;

    public Connection2()
    {
        // _channels.Add(0, new DefaultAmqpChannel(this));
    }

    private short NextChannelId()
    {
        Interlocked.Increment(ref _channelId);
        return (short)_channelId;
    }
    
    // public async Task<Channel> CreateChannelAsync()
    // {
    //     var channel = new Channel(this, NextChannelId());
    //     _channels.Add(_channelId, channel);
    //     await channel.OpenAsync();
    //
    //     return channel;
    // }

    public async Task OpenAsync()
    {
        SetupTcpClient();
        StartListener();
        _defaultChannel.SendProtocolHeader();

        queue.Take();
    }

    private void SetupTcpClient()
    {
        _tcpClient = new TcpClient("localhost", 5672);
        _amqpStreamWrapper = new AmqpStreamWrapper(_tcpClient.GetStream());
    }

    private void StartListener()
    {
        Task.Factory.StartNew(async () =>
        {
            while (true)
            {
                var frame = await _amqpStreamWrapper.ReadFrameAsync();

                switch (frame.Type)
                {
                    case FrameType.Method:
                        var methodFrame = (LowLevelAmqpMethodFrame)frame;
                        
                        var channel = _channels.ContainsKey(frame.Channel)
                            ? _channels[frame.Channel]
                            : throw new Exception($"Invalid channel: {frame.Channel}");
                        // await channel.HandleMethodFrameAsync(frame.Payload);
                        break;
                        
                    
                    default:
                        throw new Exception($"Not matched type {frame.Type}");

                }
            }
        }, TaskCreationOptions.LongRunning);
    }

    private Dictionary<short, Dictionary<string, Object>> WaiterDictionary = new()
    {
        { 0, new Dictionary<string, object>() },
    };
    
    // public async Task<TResponse> InvokeAsync<TMethod, TResponse>(TMethod method) where TMethod: Method, new()
    // {
    //     var methodBytes = Encoder.MarshalMethodFrame(method);
    //     var (classId, methodId) =  method.ClassMethodId();
    //
    //     var frame = new LowLevelAmqpMethodFrame(0,  classId, methodId, methodBytes);
    //     await _amqpStreamWrapper.SendFrameAsync(frame);
    //
    //     TaskCompletionSource <TResponse> s = new();
    //     WaiterDictionary[0].Add(nameof(TResponse), s);
    // }

    internal void OpenEnd()
    {
        queue.Add(null);
        // _openChannel.Writer.WriteAsync(0);
    }

    // FIXME
    internal async Task SendFrame(AMQPFrame frame)
    {
        await _tcpClient.Client.SendAsync(frame.ToBytes(), SocketFlags.None);
    }


    // FIXME
    internal Task Send(byte[] bytes)
    {
        _tcpClient.Client.Send(bytes);
        return Task.CompletedTask;
    }
    
    // private async Task<AmqpFrame> ReadFrameAsync()
    // {
    //     var header = await ReadAsync(7);
    //
    //     var reader = new BinReader(header);
    //     var type = reader.ReadByte();
    //     var channel = reader.ReadInt16();
    //     var size = reader.ReadInt32();
    //
    //     var frameBody = await ReadAsync(size);
    //     // Read EOF frame
    //     var end = await ReadAsync(1);
    //
    //     if (type == 1)
    //     {
    //         return new AmqpMethodFrame(channel, frameBody);
    //     }
    //     else
    //     {
    //         throw new Exception("failed");
    //     }
    // }
    //
    // private async Task<byte[]> ReadAsync(int size)
    // {
    //     int bytesRead = 0;
    //     var stream = _tcpClient.GetStream();
    //     byte[] buffer2 = new byte[size];
    //     
    //     while (bytesRead < size)
    //     {
    //         var read = await stream.ReadAsync(buffer2, bytesRead, size - bytesRead, CancellationToken.None);
    //
    //         if (read == 0)
    //         {
    //             // TODO: handle close case
    //         }
    //
    //         bytesRead += read;
    //     }
    //
    //     // while (bytesRead < size)
    //     // {
    //     //     byte[] buffer = new byte[size];
    //     //     bytesRead += await _tcpClient.Client.ReceiveAsync(buffer, SocketFlags.None);
    //     //     result = result.Concat(buffer).ToArray();
    //     // }
    //
    //     return buffer2;
    // }
}