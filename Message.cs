namespace AMQPClient;

public class Message
{
    public byte[] Data { get; }
    public MessageProperties Properties { get; }
    public MessageMetadata Metadata { get; set; }

    public Message(byte[] data): this(data, null, null)
    { }

    public Message(byte[] data, MessageProperties properties): this(data, properties, null)
    { }

    public Message(byte[] data, MessageProperties? properties, MessageMetadata? metadata)
    {
        Data = data;
        Properties = properties ?? new ();
        Metadata = metadata ?? new();
    }
}

public class MessageProperties : HeaderProperties {}

public class MessageMetadata
{
    public long DeliveryTag { get; set; }
    public bool Redelivered { get; set; }
    public String Exchange { get; set; }
    public String RoutingKey { get; set; }
}