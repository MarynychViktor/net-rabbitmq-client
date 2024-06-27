namespace AMQPClient;

public interface IMessage
{
    public byte[] Content { get; }
    public HeaderProperties? Properties { get; }
    public long? DeliveryTag { get; }
    public bool? Redelivered { get; }
    public string? Exchange { get; }
    public string? RoutingKey { get; }
}

public record Message2 : IMessage
{
    public byte[] Content { get; }
    public HeaderProperties? Properties { get; }
    public long? DeliveryTag { get; }
    public bool? Redelivered { get; }
    public string? Exchange { get; }
    public string? RoutingKey { get; }

    public Message2(byte[] content, HeaderProperties? properties = null)
    {
        Content = content;
        Properties = properties;
    }

    internal Message2(byte[] content, HeaderProperties? properties, long? deliveryTag = null, bool? redelivered = null, string? exchange = null, string? routingKey = null)
        : this(content, properties)
    {
        DeliveryTag = deliveryTag;
        Redelivered = redelivered;
        Exchange = exchange;
        RoutingKey = routingKey;
    }
}

public record IncomingMessage : Message2
{
    internal IncomingMessage(byte[] content, HeaderProperties properties, long deliveryTag, bool redelivered, string exchange, string routingKey)
        : base(content, properties, deliveryTag, redelivered, exchange, routingKey)
    {
    }
}



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