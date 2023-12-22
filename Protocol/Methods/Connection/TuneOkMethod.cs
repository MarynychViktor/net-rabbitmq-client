using System.Text;

namespace AMQPClient.Methods.Connection;

[MethodDef(classId: 10, methodId: 31)]
public class TuneOkMethod : Method
{
    [ShortField(0)]
    public short ChannelMax { get; set; }

    [IntField(1)]
    public int FrameMax { get; set; }

    [ShortField(2)]
    public short Heartbeat { get; set; }
}