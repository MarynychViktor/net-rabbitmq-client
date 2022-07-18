﻿using System.ComponentModel;
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

    private void SendProtocolHeader()
    {
        _client.Client.Send(Encoding.ASCII.GetBytes("AMQP").Concat(new byte[] { 0, 0, 9, 1 }).ToArray());
    }
    
    private async void open()
    {
        _client = new TcpClient("localhost", 5672);
        SendProtocolHeader();

        await Task.Factory.StartNew(async () =>
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
    }
    
    private async Task<AMQPFrame> ReadFrameAsync()
    {
        var header = await ReadAsync(7);

        var reader = new BinReader(new MemoryStream(header));
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