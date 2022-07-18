using System.Text;

namespace AMQPClient.Methods.Connection;

public class TuneOkMethod : Method
{
    public short ClassId => 10;
    public short MethodId => 31;

    [ShortField(0)]
    public short ChannelMax { get; set; }

    [IntField(1)]
    public int FrameMax { get; set; }

    [ShortField(2)]
    public short Heartbeat { get; set; }


    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.AppendLine($"ChannelMax: {ChannelMax}");
        builder.AppendLine($"FrameMax: {FrameMax}");
        builder.AppendLine($"Heartbeat: {Heartbeat}");
       
        return builder.ToString();
    }
}