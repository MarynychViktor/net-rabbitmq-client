using System.Text;

namespace AMQPClient.Protocol;

// FIXME: review with more efficient approach
public class BinReader : BinaryReader
{
    private readonly Stream _stream;

    public BinReader(byte[] bytes) : this(new MemoryStream(bytes))
    {
    }

    public BinReader(Stream stream) : base(stream)
    {
        _stream = stream;
    }

    public Dictionary<string, object> ReadFieldTable()
    {
        var tableSize = ReadUInt32();
        var endPos = _stream.Position + tableSize;
        Dictionary<string, object> properties = new();

        while (_stream.Position < endPos)
        {
            var (name, value) = ReadFieldValuePair();
            properties[name] = value;
        }

        return properties;
    }

    public (string, object) ReadFieldValuePair()
    {
        var fieldName = ReadShortStr();

        var val = ReadChar();

        return (fieldName, ReadFieldValue(val));
    }

    public object ReadFieldValue()
    {
        var fieldType = ReadChar();
        return ReadFieldValue(fieldType);
    }

    public string ReadShortStr()
    {
        var strLen = ReadByte();
        var bytes = ReadBytes(strLen);

        return Encoding.ASCII.GetString(bytes);
    }

    public string ReadLongStr()
    {
        long size = ReadUInt32();
        var bytes = ReadBytes((int)size);
        // return Encoding.ASCII.GetString(bytes);
        return Encoding.ASCII.GetString(bytes);
    }

    public override short ReadInt16()
    {
        var bytes = ReadBytesInMachineOrder(2);
        return BitConverter.ToInt16(bytes);
    }

    public override int ReadInt32()
    {
        var bytes = ReadBytesInMachineOrder(4);
        return BitConverter.ToInt32(bytes);
    }

    public override long ReadInt64()
    {
        var bytes = ReadBytesInMachineOrder(8);
        return BitConverter.ToInt64(bytes);
    }

    public override ushort ReadUInt16()
    {
        var bytes = ReadBytesInMachineOrder(2);
        return BitConverter.ToUInt16(bytes);
    }

    public override uint ReadUInt32()
    {
        var bytes = ReadBytesInMachineOrder(4);
        return BitConverter.ToUInt32(bytes);
    }

    public override ulong ReadUInt64()
    {
        var bytes = ReadBytesInMachineOrder(8);
        return BitConverter.ToUInt64(bytes);
    }

    public override float ReadSingle()
    {
        var bytes = ReadBytesInMachineOrder(4);
        return BitConverter.ToSingle(bytes);
    }

    public override double ReadDouble()
    {
        var bytes = ReadBytesInMachineOrder(8);
        return BitConverter.ToDouble(bytes);
    }

    public object ReadFieldValue(char type)
    {
        switch (type)
        {
            case 't':
                return ReadBoolean();
            case 'b':
                return ReadByte();
            case 'B':
                return ReadByte();
            case 'U':
                return ReadInt16();
            case 'u':
                return ReadUInt16();
            case 'I':
                return ReadInt32();
            case 'i':
                return ReadUInt32();
            case 'L':
                return ReadInt64();
            case 'l':
                return ReadUInt64();
            case 'f':
                return ReadSingle();
            case 'd':
                return ReadDouble();
            case 'D':
            case 'A':
                throw new Exception("Unsupported Decimal type");
            case 's':
                return ReadShortStr();
            case 'S':
                return ReadLongStr();
            case 'F':
                return ReadFieldTable();
            default:
                throw new Exception($"Invalid character provided `{type}`");
        }
    }

    public byte[] ReadBytesInMachineOrder(int size)
    {
        var bytes = ReadBytes(size);

        if (BitConverter.IsLittleEndian) Array.Reverse(bytes);

        return bytes;
    }

    public HeaderProperties ReadProperties()
    {
        HeaderProperties props = new();
        var flags = (HeaderPropertiesFlags)ReadUInt16();

        if ((flags & HeaderPropertiesFlags.ContentType) != 0) props.ContentType = ReadShortStr();

        if ((flags & HeaderPropertiesFlags.ContentEncoding) != 0) props.ContentEncoding = ReadShortStr();

        if ((flags & HeaderPropertiesFlags.Headers) != 0) props.Headers = ReadFieldTable();

        if ((flags & HeaderPropertiesFlags.DeliveryMode) != 0) props.DeliveryMode = (MessageDeliveryMode)ReadByte();
        // TODO: review
        // props.Headers = reader.ReadFieldTable();
        if ((flags & HeaderPropertiesFlags.Priority) != 0) props.Priority = ReadByte();

        if ((flags & HeaderPropertiesFlags.CorrelationId) != 0) props.CorrelationId = ReadShortStr();

        if ((flags & HeaderPropertiesFlags.ReplyTo) != 0) props.ReplyTo = ReadShortStr();

        if ((flags & HeaderPropertiesFlags.Expiration) != 0) props.Expiration = ReadShortStr();

        if ((flags & HeaderPropertiesFlags.MessageId) != 0) props.MessageId = ReadShortStr();

        if ((flags & HeaderPropertiesFlags.Timestamp) != 0) props.Timestamp = ReadUInt64();

        if ((flags & HeaderPropertiesFlags.Type) != 0) props.Type = ReadShortStr();

        if ((flags & HeaderPropertiesFlags.UserId) != 0) props.UserId = ReadShortStr();

        return props;
    }
}