namespace AMQPClient;

public class ChannelIdGenerator
{
    private int _channelId;

    public short GenerateChannelId()
    {
        Interlocked.Increment(ref _channelId);
        return (short)_channelId;
    }
}