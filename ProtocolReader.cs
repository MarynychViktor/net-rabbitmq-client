namespace AMQPClient;

public class ProtocolReader : BinaryReader
{
    private BinaryReader _reader;
    private Stream _stream;

    public ProtocolReader(byte[] bytes): this(new MemoryStream(bytes))
    {
    }

    public ProtocolReader(Stream stream) : base(stream)
    {
        _stream = stream;
    }

    public (byte Type, int Chan, int Size) ReadFrameHeader()
    {
        return (Type: _reader.ReadByte(), Chan: _reader.ReadInt16(), _reader.ReadInt32());
    }

    public Dictionary<string, object> ReadFieldTable()
    {
        uint tableSize = _reader.ReadUInt32();
        long endPos = _stream.Position + tableSize;
        Dictionary<string, object> properties = new();

        while (endPos < _stream.Position)
        {
            var (name, value) = ReadFieldValuePair();
            properties[name] = value;
        }

        return properties;
    }

    public (string, object) ReadFieldValuePair()
    {
        string fieldName = ReadShortStr();
        char val = _reader.ReadChar();

        return (fieldName, ReadFieldValue(val));
    }

    public object ReadFieldValue(char type)
    {
        uint fieldType = _reader.ReadChar();
        switch (fieldType)
        {
            case 't':
                return _reader.ReadBoolean();
            
            case 'b':
                return _reader.ReadByte();
            case 'B':
                return _reader.ReadByte();
            case 'U':
                return _reader.ReadInt16();
            case 'u':
                return _reader.ReadUInt16();
            case 'I':
                return _reader.ReadInt32();
            case 'i':
                return _reader.ReadUInt32();
            case 'L':
                return _reader.ReadInt64();
            case 'l':
                return _reader.ReadUInt64();
            case 'f':
                return _reader.ReadSingle();
            case 'd':
                return _reader.ReadDouble();
            case 'D': case 'A':
                throw new Exception("Unsupported Decimal type");
            case 's':
                return ReadShortStr();
            case 'S':
                return ReadLongStr();
            case 'F':
                return ReadFieldTable();
            default:
                throw new Exception($"Invalid character provided {type}");
        }
    }

    public object ReadFieldValue()
    {
        char fieldType = _reader.ReadChar();
        return ReadFieldValue(fieldType);
    }

    public string ReadShortStr()
    {
        byte strLen = _reader.ReadByte();
        byte[] items = _reader.ReadBytes(strLen);

        return BitConverter.ToString(items);
    }

    public string ReadLongStr()
    {
        long size = _reader.ReadUInt32();
        byte[] bytes = _reader.ReadBytes((int)size);

        return BitConverter.ToString(bytes);
    }
    
    public bool BoolFromBytes(byte[] bytes)
    {
        return BitConverter.ToBoolean(bytes);
    }
    // func FieldValueFromBytes(b []byte, code rune) (any, uint) {
    //     switch code {
    //         case 't':
    //         return BoolFromBytes(b)
    //         case 'b':
    //         return Int8FromBytes(b)
    //         case 'B':
    //         return Uint8FromBytes(b)
    //         case 'U':
    //         return Int16FromBytes(b)
    //         case 'u':
    //         return Uint16FromBytes(b)
    //         case 'I':
    //         return Int32FromBytes(b)
    //         case 'i':
    //         return Uint32FromBytes(b)
    //         case 'L':
    //         return Int64FromBytes(b)
    //         case 'l':
    //         return Uint64FromBytes(b)
    //         case 'f':
    //         return Float32FromBytes(b)
    //         case 'd':
    //         return Float64FromBytes(b)
    //         case 'D':
    //         return DecimalFromBytes(b)
    //         case 's':
    //         return ShortStringFromBytes(b)
    //         case 'S':
    //         return LongStringFromBytes(b)
    //         case 'F':
    //         return FieldTableFromBytes(b)
    //         case 'A':
    //         panic("failed to handle field-array")
    //         //	fmt.Println("field-array")
    //         //	s, _ := numbers.ByteToUint32(b[1:5])
    //         //	read = 0
    //         //	FieldValue(b[5 : s+5], 0)
    //
    //         case 'T':
    //         return TimestampFromBytes(b)
    //         case 'V':
    //         return 0, 0
    //         default:
    //         fmt.Println("failed to process type ", code, string(code))
    //         panic("failed process type ")
    //     }
    // }

}