using System.Text;
using AMQPClient.Protocol.Methods;

namespace AMQPClient.Protocol.Types;

public class AmqpEnvelope
{
    public Method Method { get; }
    public AmqpEnvelopePayload? Payload { get; }

    public AmqpEnvelope(Method method, AmqpEnvelopePayload? payload = null)
    {
        Method = method;
        Payload = payload;
    }
    
    public override string ToString()
    {
        return $"Method: {Method} \n" +
               $"Payload: {Payload}";
    }
}

public class AmqpEnvelopePayload
{
    public HeaderProperties Properties { get; }
    public byte[] Content { get; }

    public AmqpEnvelopePayload(HeaderProperties properties, byte[] content)
    {
        Properties = properties;
        Content = content;
    }

    public override string ToString()
    {
        return $"Properties: {Properties} \n" +
               $"Body: {Encoding.UTF8.GetString(Content)}";
    }
}
//
// [Flags]
// public enum AmqpPropertiesFlags
// {
//     None = 0,
//     ContentType = 32768,
//     ContentEncoding = 16384,
//     Headers = 8192,
//     DeliveryMode = 4096,
//     Priority = 2048,
//     CorrelationId = 1024,
//     ReplyTo = 512,
//     Expiration = 256,
//     MessageId = 128,
//     Timestamp = 64,
//     Type = 32,
//     UserId = 16,
//     AppId = 8,
// }
//
// public class AmqpPayloadProperties
// {
//     public String? ContentType { get; set; }
//     public String? ContentEncoding { get; set; }
//     public Dictionary<string, Object>? Headers { get; set; }
//     public MessageDeliveryMode? DeliveryMode { get; set; }
//     public byte? Priority { get; set; }
//     public String? CorrelationId { get; set; }
//     public String? ReplyTo { get; set; }
//     // TODO: review
//     public String? Expiration { get; set; }
//     public String? MessageId { get; set; }
//     public ulong? Timestamp { get; set; }
//     public String? Type { get; set; }
//     public String? UserId { get; set; }
//     public String? AppId { get; set; }
//     public String Reserved { get; set; } = "";
//
//     public static AmqpPayloadProperties FromRaw(byte[] bytes)
//     {
//         using var reader = new BinReader(bytes);
//         AmqpPayloadProperties props = new();
//         AmqpPropertiesFlags flags = (AmqpPropertiesFlags)reader.ReadUInt16();
//
//         if ((flags & AmqpPropertiesFlags.ContentType) != 0)
//         {
//             props.ContentType = reader.ReadShortStr();
//         }
//         
//         if ((flags & AmqpPropertiesFlags.ContentEncoding) != 0)
//         {
//             props.ContentEncoding = reader.ReadShortStr();
//         }
//                 
//         if ((flags & AmqpPropertiesFlags.Headers) != 0)
//         {
//             props.Headers = reader.ReadFieldTable();
//         }
//                         
//         if ((flags & AmqpPropertiesFlags.DeliveryMode) != 0)
//         {
//             props.DeliveryMode = (MessageDeliveryMode)reader.ReadByte();
//         }
//                         
//         if ((flags & AmqpPropertiesFlags.Priority) != 0)
//         {
//             props.Priority = reader.ReadByte();
//         }
//                                 
//         if ((flags & AmqpPropertiesFlags.CorrelationId) != 0)
//         {
//             props.CorrelationId = reader.ReadShortStr();
//         }
//         
//         if ((flags & AmqpPropertiesFlags.ReplyTo) != 0)
//         {
//             props.ReplyTo = reader.ReadShortStr();
//         }   
//
//         if ((flags & AmqpPropertiesFlags.Expiration) != 0)
//         {
//             props.Expiration = reader.ReadShortStr();
//         }   
//
//         if ((flags & AmqpPropertiesFlags.MessageId) != 0)
//         {
//             props.MessageId = reader.ReadShortStr();
//         }   
//
//         if ((flags & AmqpPropertiesFlags.Timestamp) != 0)
//         {
//             props.Timestamp = reader.ReadUInt64();
//         }  
//         
//         if ((flags & AmqpPropertiesFlags.Type) != 0)
//         {
//             props.Type = reader.ReadShortStr();
//         }  
//
//         if ((flags & AmqpPropertiesFlags.UserId) != 0)
//         {
//             props.UserId = reader.ReadShortStr();
//         }
//
//         if ((flags & AmqpPropertiesFlags.AppId) != 0)
//         {
//             props.AppId = reader.ReadShortStr();
//         }
//
//         return props;
//     }
//     
//     public  byte[] ToRaw(byte[] bytes)
//     {
//         using var flagWriter = new BinWriter();
//         using var valueWriter = new BinWriter();
//         AmqpPayloadProperties props = new();
//         AmqpPropertiesFlags flags = AmqpPropertiesFlags.None;
//
//         if (props.ContentType != null)
//         {
//             flags &= AmqpPropertiesFlags.ContentType;
//             valueWriter.WriteShortStr(props.ContentType);
//         }
//          
//         if (props.ContentEncoding != null)
//         {
//             flags &= AmqpPropertiesFlags.ContentEncoding;
//             valueWriter.WriteShortStr(props.ContentEncoding);
//         }
//                  
//         if (props.Headers != null)
//         {
//             flags &= AmqpPropertiesFlags.Headers;
//             valueWriter.WriteFieldTable(props.Headers);
//         }
//
//         if (props.DeliveryMode != null)
//         {
//             flags &= AmqpPropertiesFlags.DeliveryMode;
//             valueWriter.Write((byte)props.DeliveryMode);
//         }   
//                                      
//         if (props.Priority != null)
//         {
//             flags &= AmqpPropertiesFlags.Priority;
//             valueWriter.Write((byte)props.Priority);
//         }          
//
//         if (props.CorrelationId != null)
//         {
//             flags &= AmqpPropertiesFlags.CorrelationId;
//             valueWriter.WriteShortStr(props.CorrelationId);
//         }                         
//  
//         if (props.ReplyTo != null)
//         {
//             flags &= AmqpPropertiesFlags.ReplyTo;
//             valueWriter.WriteShortStr(props.ReplyTo);
//         }             
//  
//         if (props.Expiration != null)
//         {
//             flags &= AmqpPropertiesFlags.Expiration;
//             valueWriter.WriteShortStr(props.Expiration);
//         } 
//  
//         if (props.MessageId != null)
//         {
//             flags &= AmqpPropertiesFlags.MessageId;
//             valueWriter.WriteShortStr(props.MessageId);
//         } 
//  
//         if (props.Timestamp != null)
//         {
//             flags &= AmqpPropertiesFlags.Timestamp;
//             valueWriter.Write((ulong)props.Timestamp);
//         } 
//          
//         if (props.Type != null)
//         {
//             flags &= AmqpPropertiesFlags.Type;
//             valueWriter.WriteShortStr(props.Type);
//         } 
//         
//         if (props.UserId != null)
//         {
//             flags &= AmqpPropertiesFlags.UserId;
//             valueWriter.WriteShortStr(props.UserId);
//         } 
//         
//         if (props.AppId != null)
//         {
//             flags &= AmqpPropertiesFlags.AppId;
//             valueWriter.WriteShortStr(props.AppId);
//         }
//
//         flagWriter.Write(((ushort)flags));
//
//         return flagWriter.ToArray().Concat(valueWriter.ToArray()).ToArray();
//     }
// }

public enum MessageDeliveryMode
{
    NonPersistent = 1,
    Persistent = 2,
}