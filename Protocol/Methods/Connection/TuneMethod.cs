using AMQPClient.Protocol.Attributes;

namespace AMQPClient.Protocol.Methods.Connection;

[MethodDef(classId: 10, methodId: 30)]
public class TuneMethod : Method
{
    [ShortField(0)]
    public short ChannelMax { get; set; }

    [IntField(1)]
    public int FrameMax { get; set; }

    [ShortField(2)]
    public short Heartbeat { get; set; }
}