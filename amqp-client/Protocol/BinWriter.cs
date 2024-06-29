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
        Write((uint)fieldTable.Length);
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
        Write((uint)bytes.Length);
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
                Write(i);
                break;
            case ushort i:
                Write('u');
                Write(i);
                break;
            case int i:
                Write('I');
                Write(i);
                break;
            case uint i:
                Write('i');
                Write(i);
                break;
            case long i:
                Write('L');
                Write(i);
                break;
            case ulong i:
                Write('l');
                Write(i);
                break;
            case float i:
                Write('f');
                Write(i);
                break;
            case double i:
                Write('d');
                Write(i);
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

    public override void Write(short value)
    {
        WriteInBigEndian(BitConverter.GetBytes(value));
    }

    public override void Write(ushort value)
    {
        WriteInBigEndian(BitConverter.GetBytes(value));
    }

    public override void Write(int value)
    {
        WriteInBigEndian(BitConverter.GetBytes(value));
    }

    public override void Write(uint value)
    {
        WriteInBigEndian(BitConverter.GetBytes(value));
    }

    public override void Write(long value)
    {
        WriteInBigEndian(BitConverter.GetBytes(value));
    }

    public override void Write(ulong value)
    {
        WriteInBigEndian(BitConverter.GetBytes(value));
    }

    public override void Write(float value)
    {
        WriteInBigEndian(BitConverter.GetBytes(value));
    }

    public override void Write(double value)
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