using System.Text;

namespace AMQPClient.Protocol;

public class BinWriter : BinaryWriter
{
    private readonly Stream _stream;

    public BinWriter() : this(new MemoryStream())
    {
    }

    public BinWriter(Stream stream) : base(stream)
    {
        _stream = stream;
    }

    public void WriteFieldTable(Dictionary<string, object> table)
    {
        BinWriter tableWriter = new();

        foreach (var (k, v) in table) tableWriter.WriteFieldValuePair(k, v);

        var fieldTable = ((MemoryStream)tableWriter.OutStream).ToArray();
        WriteUint((uint)fieldTable.Length);
        Write(fieldTable);
    }

    public void WriteFieldValuePair(string key, object value)
    {
        WriteShortStr(key);
        WriteFieldValue(value);
    }

    public void WriteShortStr(string s)
    {
        var bytes = Encoding.ASCII.GetBytes(s);
        Write((byte)bytes.Length);
        Write(bytes);
    }

    public void WriteLongStr(string s)
    {
        var bytes = Encoding.ASCII.GetBytes(s);
        WriteUint((uint)bytes.Length);
        Write(bytes);
    }

    public void WriteFieldValue(object value)
    {
        switch (value)
        {
            case bool item:
                Write('t');
                Write(item);
                break;
            case byte i:
                Write('b');
                Write(i);
                break;
            case short i:
                Write('U');
                WriteShort(i);
                break;
            case ushort i:
                Write('u');
                WriteUshort(i);
                break;
            case int i:
                Write('I');
                WriteInt(i);
                break;
            case uint i:
                Write('i');
                WriteUint(i);
                break;
            case long i:
                Write('L');
                WriteLong(i);
                break;
            case ulong i:
                Write('l');
                WriteUlong(i);
                break;
            case float i:
                Write('f');
                WriteFloat(i);
                break;
            case double i:
                Write('d');
                WriteDouble(i);
                break;
            case string s:
                Write('S');
                WriteLongStr(s);
                break;
            case Dictionary<string, object> d:
                Write('F');
                WriteFieldTable(d);
                break;
            default:
                throw new Exception("Failed to write value");
        }
    }
    public void WriteByte(byte value)
    {
        Write(value);
    }

    public void WriteShort(short value)
    {
        WriteInBigEndian(BitConverter.GetBytes(value));
    }

    public void WriteUshort(ushort value)
    {
        WriteInBigEndian(BitConverter.GetBytes(value));
    }

    public void WriteInt(int value)
    {
        WriteInBigEndian(BitConverter.GetBytes(value));
    }

    public void WriteUint(uint value)
    {
        WriteInBigEndian(BitConverter.GetBytes(value));
    }

    public void WriteLong(long value)
    {
        WriteInBigEndian(BitConverter.GetBytes(value));
    }

    public void WriteUlong(ulong value)
    {
        WriteInBigEndian(BitConverter.GetBytes(value));
    }

    public void WriteFloat(float value)
    {
        WriteInBigEndian(BitConverter.GetBytes(value));
    }

    public void WriteDouble(double value)
    {
        WriteInBigEndian(BitConverter.GetBytes(value));
    }

    public void WriteBit(byte value, bool appendToPrevious = false)
    {
        if (appendToPrevious)
        {
            _stream.Seek(-1, SeekOrigin.Current);
            var prevValue = _stream.ReadByte();
            _stream.Seek(-1, SeekOrigin.Current);
            Write((byte)(value | prevValue));
        }
        else
        {
            Write(value);
        }
    }

    public byte[] ToArray()
    {
        if (_stream is MemoryStream memoryStream) return memoryStream.ToArray();

        throw new Exception("Not supported method");
    }

    private void WriteInBigEndian(byte[] bytes)
    {
        Array.Reverse(bytes);
        Write(bytes);
    }

    private void WriteInBigEndian(Span<byte> bytes)
    {
        bytes.Reverse();
        Write(bytes);
    }
}