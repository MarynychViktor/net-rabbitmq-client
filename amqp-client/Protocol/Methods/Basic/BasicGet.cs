using AMQPClient.Protocol.Attributes;

namespace AMQPClient.Protocol.Methods.Basic;

[MethodDef(60, 70)]
public class BasicGet: Method
{    
    [ShortField(0)]
    public short Reserved1 { get; set; }

    [ShortStringField(1)]
    public string QueueName { get; set; }

    [ByteField(2)]
    public byte NoAck { get; set; }
}