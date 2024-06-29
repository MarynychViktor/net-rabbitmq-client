using AMQPClient.Protocol;

public record QueueDeclare {
	public const short ClassId = 50;
	public const short MethodId = 10;
	public const bool IsAsyncResponse = false;
	public const bool HasBody = false;

	public short Reserved1 {get;set;}
	public string Queue {get;set;}
	public bool Passive {get;set;}
	public bool Durable {get;set;}
	public bool Exclusive {get;set;}
	public bool AutoDelete {get;set;}
	public byte NoWait {get;set;}
	public Dictionary<string, object> Arguments {get;set;}

	public void Serialize() {
		var writer = new BinWriter();
		writer.Write(ClassId);
		writer.Write(MethodId);
		writer.WriteShort(Reserved1);
		writer.WriteShortstr(Queue);
		writer.WriteBit(Passive);
		writer.WriteBit(Durable);
		writer.WriteBit(Exclusive);
		writer.WriteBit(AutoDelete);
		writer.WriteNoWait(NoWait, true);
		writer.WriteTable(Arguments);
		return writer.ToArray();
	}

	public void Deserialize(byte[] bytes) {
		var reader = new BinReader(bytes);
		reader.ReadShort(ClassId);
		reader.ReadShort(MethodId);
		Queue = reader.ReadShortstr();
		var flags = reader.ReadBit();
		Passive = flags & 1 > 0;
		Durable = flags & 2 > 0;
		Exclusive = flags & 4 > 0;
		AutoDelete = flags & 8 > 0;
		NoWait = flags & 16 > 0;
		Arguments = reader.ReadTable();
	}
}}
