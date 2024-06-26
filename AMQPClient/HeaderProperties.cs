using System.Buffers.Binary;
using AMQPClient.Protocol;

namespace AMQPClient;

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
    AppId = 8
}

public class HeaderProperties
{
    public string? ContentType { get; set; }
    public string? ContentEncoding { get; set; }
    public Dictionary<string, object>? Headers { get; set; }
    public MessageDeliveryMode? DeliveryMode { get; set; }
    public byte? Priority { get; set; }
    public string? CorrelationId { get; set; }

    public string? ReplyTo { get; set; }

    // TODO: review
    public string? Expiration { get; set; }
    public string? MessageId { get; set; }
    public ulong? Timestamp { get; set; }
    public string? Type { get; set; }
    public string? UserId { get; set; }
    public string? AppId { get; set; }
    public string Reserved { get; set; } = "";

    public override string ToString()
    {
        return
            $"{nameof(ContentType)}: {ContentType}, {nameof(ContentEncoding)}: {ContentEncoding}, {nameof(Headers)}: {Headers}, {nameof(DeliveryMode)}: {DeliveryMode}, {nameof(Priority)}: {Priority}, {nameof(CorrelationId)}: {CorrelationId}, {nameof(ReplyTo)}: {ReplyTo}, {nameof(Expiration)}: {Expiration}, {nameof(MessageId)}: {MessageId}, {nameof(Timestamp)}: {Timestamp}, {nameof(Type)}: {Type}, {nameof(UserId)}: {UserId}, {nameof(AppId)}: {AppId}, {nameof(Reserved)}: {Reserved}";
    }

    public static HeaderProperties FromRaw(byte[] bytes)
    {
        using var reader = new BinReader(bytes);
        HeaderProperties props = new();
        var flags = (HeaderPropertiesFlags)reader.ReadUshort();

        if ((flags & HeaderPropertiesFlags.ContentType) != 0) props.ContentType = reader.ReadShortStr();

        if ((flags & HeaderPropertiesFlags.ContentEncoding) != 0) props.ContentEncoding = reader.ReadShortStr();

        if ((flags & HeaderPropertiesFlags.Headers) != 0) props.Headers = reader.ReadFieldTable();

        if ((flags & HeaderPropertiesFlags.DeliveryMode) != 0)
            props.DeliveryMode = (MessageDeliveryMode)reader.ReadByte();
        // TODO: review
        // props.Headers = reader.ReadFieldTable();
        if ((flags & HeaderPropertiesFlags.Priority) != 0) props.Priority = reader.ReadByte();

        if ((flags & HeaderPropertiesFlags.CorrelationId) != 0) props.CorrelationId = reader.ReadShortStr();

        if ((flags & HeaderPropertiesFlags.ReplyTo) != 0) props.ReplyTo = reader.ReadShortStr();

        if ((flags & HeaderPropertiesFlags.Expiration) != 0) props.Expiration = reader.ReadShortStr();

        if ((flags & HeaderPropertiesFlags.MessageId) != 0) props.MessageId = reader.ReadShortStr();

        if ((flags & HeaderPropertiesFlags.Timestamp) != 0) props.Timestamp = reader.ReadUlong();

        if ((flags & HeaderPropertiesFlags.Type) != 0) props.Type = reader.ReadShortStr();

        if ((flags & HeaderPropertiesFlags.UserId) != 0) props.UserId = reader.ReadShortStr();

        return props;
    }

    public byte[] ToRaw()
    {
        Span<byte> bytes = stackalloc byte[2];
        using var valueWriter = new BinWriter();
        HeaderProperties props = new();
        var flags = HeaderPropertiesFlags.None;

        // reserve 2 bytes for flags
        valueWriter.Write(bytes);

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
            valueWriter.WriteUlong((ulong)props.Timestamp);
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

        var result = valueWriter.ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan()[..2], (ushort)flags);
        return result;
    }
}

public enum MessageDeliveryMode
{
    NonPersistent = 1,
    Persistent = 2
}