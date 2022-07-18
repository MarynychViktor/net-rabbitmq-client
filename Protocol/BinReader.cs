using System.Text;

namespace AMQPClient.Protocol;

public class BinReader : BinaryReader
{
    private Stream _stream;

    public BinReader(byte[] bytes): this(new MemoryStream(bytes))
    {
    }

    public BinReader(Stream stream) : base(stream)
    {
        _stream = stream;
    }

    public Dictionary<string, object> ReadFieldTable()
    {
        uint tableSize = ReadUInt32();
        long endPos = _stream.Position + tableSize;
        Dictionary<string, object> properties = new();
        Console.WriteLine($"Tab size {tableSize}, endPos {endPos}, pos {_stream.Position}");

        while (_stream.Position < endPos)
        {
            Console.WriteLine($"Tab size {tableSize}, endPos {endPos}, pos {_stream.Position}");
            var (name, value) = ReadFieldValuePair();
            properties[name] = value;
        }

        return properties;
    }

    public (string, object) ReadFieldValuePair()
    {
        string fieldName = ReadShortStr();
        Console.WriteLine($"fieldName {fieldName}");

        char val = ReadChar();

        return (fieldName, ReadFieldValue(val));
    }

    public object ReadFieldValue()
    {
        char fieldType = ReadChar();
        return ReadFieldValue(fieldType);
    }

    public string ReadShortStr()
    {
        byte strLen = ReadByte();
        Console.WriteLine($"Str len {strLen}");
        byte[] bytes = ReadBytes(strLen);

        return Encoding.ASCII.GetString(bytes);
    }

    public string ReadLongStr()
    {
        long size = ReadUInt32();
        byte[] bytes = ReadBytes((int)size);
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
            case 'D': case 'A':
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

        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }

        return bytes;
    }
}