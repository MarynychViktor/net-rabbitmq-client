using AMQPClient.Protocol.Attributes;

namespace AMQPClient.Protocol.Methods.Queues;

[MethodDef(50, 40)]
public class QueueDelete : Method
{
    [ShortField(0)] public short Reserved1 { get; set; }

    [ShortStringField(1)] public string Queue { get; set; }

    [ByteField(2)] public byte Flags { get; set; }
}

[Flags]
public enum QueueDeleteFlags
{
    None = 0,
    IfUnused = 1,
    IfEmpty = 2,
    NoWait = 1 << 2,
}

public class QueueDelete2
{
    public const short ClassId = 50;
    public const short MethodId = 40;
    public const bool IsAsyncResponse = false;
    public const bool HasBody = false;

    public short Reserved1 { get; set; }
    public string QueueName { get; set; }
    public bool IfUnused { get; set; }
    public bool IfEmpty { get; set; }
    public bool NoWait { get; set; }

    public byte[] Serialize()
    {
        var writer = new BinWriter();
        writer.WriteShort(ClassId);
        writer.WriteShort(MethodId);
        writer.WriteShort(Reserved1);
        writer.WriteShortStr(QueueName);
        var flags = QueueDeleteFlags.None;
        if (IfUnused) flags |= QueueDeleteFlags.IfUnused;
        if (IfEmpty) flags |= QueueDeleteFlags.IfEmpty;
        if (NoWait) flags |= QueueDeleteFlags.NoWait;
        writer.Write((byte)flags);
        return writer.ToArray();
    }

    public void Accept(IVisitor visitor)
    {
        // var writer = new BinWriter();
        // visitor.VisitShort(ClassId);
        // visitor.VisitShort(MethodId);
        // visitor.VisitShort(Reserved1);
        // visitor.VisitShortStr(QueueName);
        // visitor.VisitBit(IfUnused, true);
        // visitor.VisitBit(IfEmpty, true);
        // visitor.VisitBit(NoWait, true);
        // // writer.Write(ClassId);
        // // writer.Write(MethodId);
        // // writer.Write(Reserved1);
        // writer.WriteShortStr(QueueName);
        // var flags = QueueDeleteFlags.None;
        // if (IfUnused) flags |= QueueDeleteFlags.IfUnused;
        // if (IfEmpty) flags |= QueueDeleteFlags.IfEmpty;
        // if (NoWait) flags |= QueueDeleteFlags.NoWait;
        // writer.Write((byte)flags);
        // return writer.ToArray();
    }
}