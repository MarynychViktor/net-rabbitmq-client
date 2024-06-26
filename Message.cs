namespace AMQPClient;

public class Message
{
    public Message(byte[] data) : this(data, null, null)
    {
    }

    public Message(byte[] data, MessageProperties properties) : this(data, properties, null)
    {
    }

    public Message(byte[] data, MessageProperties? properties, MessageMetadata? metadata)
    {
        Data = data;
        Properties = properties ?? new MessageProperties();
        Metadata = metadata ?? new MessageMetadata();
    }

    public byte[] Data { get; }
    public MessageProperties Properties { get; }
    public MessageMetadata Metadata { get; set; }
}

public class MessageProperties : HeaderProperties
{
}

public class MessageMetadata
{
    public long DeliveryTag { get; set; }
    public bool Redelivered { get; set; }
    public string Exchange { get; set; }
    public string RoutingKey { get; set; }
}