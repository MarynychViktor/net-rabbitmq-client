namespace AMQPClient.Methods.Channels;

public class CloseMethod : Method
{
    public short ClassId => 20;
    public short MethodId => 40;

    [ShortField(0)]
    public short ReplyCode { get; set; } = 320;

    [ShortStringField(1)]
    public string ReplyText { get; set; } = "Closed by peer";

    [ShortField(2)]
    public short ReasonClassId { get; set; } = 0;

    [ShortField(3)]
    public short ReasonMethodId { get; set; } = 0;
}