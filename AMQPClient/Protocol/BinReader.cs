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
        var tableSize = ReadUint();
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
        long size = ReadUint();
        var bytes = ReadBytes((int)size);
        return Encoding.ASCII.GetString(bytes);
    }

    public short ReadShort()
    {
        var bytes = ReadBytesInMachineOrder(2);
        return BitConverter.ToInt16(bytes);
    }

    public int ReadInt()
    {
        var bytes = ReadBytesInMachineOrder(4);
        return BitConverter.ToInt32(bytes);
    }

    public long ReadLong()
    {
        var bytes = ReadBytesInMachineOrder(8);
        return BitConverter.ToInt64(bytes);
    }

    public ushort ReadUshort()
    {
        var bytes = ReadBytesInMachineOrder(2);
        return BitConverter.ToUInt16(bytes);
    }

    public uint ReadUint()
    {
        var bytes = ReadBytesInMachineOrder(4);
        return BitConverter.ToUInt32(bytes);
    }

    public ulong ReadUlong()
    {
        var bytes = ReadBytesInMachineOrder(8);
        return BitConverter.ToUInt64(bytes);
    }

    public float ReadFloat()
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
                return ReadShort();
            case 'u':
                return ReadUshort();
            case 'I':
                return ReadInt();
            case 'i':
                return ReadUint();
            case 'L':
                return ReadLong();
            case 'l':
                return ReadUlong();
            case 'f':
                return ReadFloat();
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
        var flags = (HeaderPropertiesFlags)ReadUshort();

        if ((flags & HeaderPropertiesFlags.ContentType) != 0) props.ContentType = ReadShortStr();

        if ((flags & HeaderPropertiesFlags.ContentEncoding) != 0) props.ContentEncoding = ReadShortStr();

        if ((flags & HeaderPropertiesFlags.Headers) != 0) props.Headers = ReadFieldTable();

        if ((flags & HeaderPropertiesFlags.DeliveryMode) != 0) props.DeliveryMode = (MessageDeliveryMode)ReadByte();

        if ((flags & HeaderPropertiesFlags.Priority) != 0) props.Priority = ReadByte();

        if ((flags & HeaderPropertiesFlags.CorrelationId) != 0) props.CorrelationId = ReadShortStr();

        if ((flags & HeaderPropertiesFlags.ReplyTo) != 0) props.ReplyTo = ReadShortStr();

        if ((flags & HeaderPropertiesFlags.Expiration) != 0) props.Expiration = ReadShortStr();

        if ((flags & HeaderPropertiesFlags.MessageId) != 0) props.MessageId = ReadShortStr();

        if ((flags & HeaderPropertiesFlags.Timestamp) != 0) props.Timestamp = ReadUlong();

        if ((flags & HeaderPropertiesFlags.Type) != 0) props.Type = ReadShortStr();

        if ((flags & HeaderPropertiesFlags.UserId) != 0) props.UserId = ReadShortStr();

        return props;
    }
}