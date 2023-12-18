using AMQPClient.Protocol;

namespace AMQPClient;

public class InternalMessage
{
    public short ChannelId { get; set; }
    public HeaderProperties HeaderProperties { get; set; }
    public MessageMetadata Metadata { get; set; }
    public byte[] Body { get; set; }
}

[Flags]
public enum HeaderPropertiesFlags
{
    None = 0,
    ContentType = 32768,
    ContentEncoding = 16384,
    Headers = 8192,
    DeliveryMode = 4096,
    Priority = 2048,
    CorrelationId = 1024,
    ReplyTo = 512,
    Expiration = 256,
    MessageId = 128,
    Timestamp = 64,
    Type = 32,
    UserId = 16,
    AppId = 8,
}

public class HeaderProperties
{
    public String? ContentType { get; set; }
    public String? ContentEncoding { get; set; }
    public Dictionary<string, Object>? Headers { get; set; }
    public MessageDeliveryMode? DeliveryMode { get; set; }
    public byte? Priority { get; set; }
    public String? CorrelationId { get; set; }
    public String? ReplyTo { get; set; }
    // TODO: review
    public String? Expiration { get; set; }
    public String? MessageId { get; set; }
    public ulong? Timestamp { get; set; }
    public String? Type { get; set; }
    public String? UserId { get; set; }
    public String? AppId { get; set; }
    public String Reserved { get; set; } = "";

    public static HeaderProperties FromRaw(byte[] bytes)
    {
        using var reader = new BinReader(bytes);
        HeaderProperties props = new();
        HeaderPropertiesFlags flags = (HeaderPropertiesFlags)reader.ReadUInt16();

        if ((flags & HeaderPropertiesFlags.ContentType) != 0)
        {
            props.ContentType = reader.ReadShortStr();
        }
        
        if ((flags & HeaderPropertiesFlags.ContentEncoding) != 0)
        {
            props.ContentEncoding = reader.ReadShortStr();
        }
                
        if ((flags & HeaderPropertiesFlags.Headers) != 0)
        {
            props.Headers = reader.ReadFieldTable();
        }
                        
        if ((flags & HeaderPropertiesFlags.DeliveryMode) != 0)
        {
            props.DeliveryMode = (MessageDeliveryMode)reader.ReadByte();
            // TODO: review
            props.Headers = reader.ReadFieldTable();
        }
                        
        if ((flags & HeaderPropertiesFlags.Priority) != 0)
        {
            props.Priority = reader.ReadByte();
        }
                                
        if ((flags & HeaderPropertiesFlags.CorrelationId) != 0)
        {
            props.CorrelationId = reader.ReadShortStr();
        }
        
        if ((flags & HeaderPropertiesFlags.ReplyTo) != 0)
        {
            props.ReplyTo = reader.ReadShortStr();
        }   

        if ((flags & HeaderPropertiesFlags.Expiration) != 0)
        {
            props.Expiration = reader.ReadShortStr();
        }   

        if ((flags & HeaderPropertiesFlags.MessageId) != 0)
        {
            props.MessageId = reader.ReadShortStr();
        }   

        if ((flags & HeaderPropertiesFlags.Timestamp) != 0)
        {
            props.Timestamp = reader.ReadUInt64();
        }  
        
        if ((flags & HeaderPropertiesFlags.Type) != 0)
        {
            props.Type = reader.ReadShortStr();
        }  

        if ((flags & HeaderPropertiesFlags.UserId) != 0)
        {
            props.UserId = reader.ReadShortStr();
        }

        return props;
    }
    
    public  byte[] ToRaw(byte[] bytes)
    {
        using var flagWriter = new BinWriter();
        using var valueWriter = new BinWriter();
        HeaderProperties props = new();
        HeaderPropertiesFlags flags = HeaderPropertiesFlags.None;
            // (HeaderPropertiesFlags)reader.ReadUInt16();

        if (props.ContentType != null)
        {
            flags &= HeaderPropertiesFlags.ContentType;
            valueWriter.WriteShortStr(props.ContentType);
        }
         
        if (props.ContentEncoding != null)
        {
            flags &= HeaderPropertiesFlags.ContentEncoding;
            valueWriter.WriteShortStr(props.ContentEncoding);
        }
                 
        if (props.Headers != null)
        {
            flags &= HeaderPropertiesFlags.Headers;
            valueWriter.WriteFieldTable(props.Headers);
        }

        if (props.DeliveryMode != null)
        {
            flags &= HeaderPropertiesFlags.DeliveryMode;
            valueWriter.Write((byte)props.DeliveryMode);
        }   
                                     
        if (props.Priority != null)
        {
            flags &= HeaderPropertiesFlags.Priority;
            valueWriter.Write((byte)props.Priority);
        }          

        if (props.CorrelationId != null)
        {
            flags &= HeaderPropertiesFlags.CorrelationId;
            valueWriter.WriteShortStr(props.CorrelationId);
        }                         
 
        if (props.ReplyTo != null)
        {
            flags &= HeaderPropertiesFlags.ReplyTo;
            valueWriter.WriteShortStr(props.ReplyTo);
        }             
 
        if (props.Expiration != null)
        {
            flags &= HeaderPropertiesFlags.Expiration;
            valueWriter.WriteShortStr(props.Expiration);
        } 
 
        if (props.MessageId != null)
        {
            flags &= HeaderPropertiesFlags.MessageId;
            valueWriter.WriteShortStr(props.MessageId);
        } 
 
        if (props.Timestamp != null)
        {
            flags &= HeaderPropertiesFlags.Timestamp;
            valueWriter.Write((ulong)props.Timestamp);
        } 
         
        if (props.Type != null)
        {
            flags &= HeaderPropertiesFlags.Type;
            valueWriter.WriteShortStr(props.Type);
        } 
        
        if (props.UserId != null)
        {
            flags &= HeaderPropertiesFlags.UserId;
            valueWriter.WriteShortStr(props.UserId);
        } 

        flagWriter.Write(((ushort)flags));

        return flagWriter.ToArray().Concat(valueWriter.ToArray()).ToArray();
    }
}

enum MessageDeliveryMode
{
    NonPersistent = 1,
    Persistent = 2,
}

public class MessageMetadata
{
    public long DeliveryTag { get; set; }
    public bool Redelivered { get; set; }
    public String Exchange { get; set; }
    public String RoutingKey { get; set; }
}