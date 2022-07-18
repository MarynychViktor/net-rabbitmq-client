using System.Net.Sockets;
using AMQPClient.Methods.Channels;
using AMQPClient.Protocol;

namespace AMQPClient;

public class Channel
{
    private short _channelId;
    private TcpClient _client;

    public Channel(TcpClient client, short id)
    {
        _client = client;
        _channelId = id;
    }

    internal Task OpenAsync()
    {
        var m = new OpenMethod();
        var frame = new AMQPFrame()
        {
            Type = AMQPFrameType.Method,
            Channel = (short)_channelId,
            Body = Encoder.MarshalMethodFrame(m),
        }.ToBytes();
        

        _client.Client.Send(frame);

        return Task.CompletedTask;
    }
}