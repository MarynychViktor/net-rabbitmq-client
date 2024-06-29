using AMQPClient.Protocol;

public class Queue {
	public class Declare {
		public const short SourceClassId = 50;
		public const short SourceMethodId = 10;
		public const bool IsAsyncResponse = false;
		public const bool HasBody = false;

		public short Reserved1 {get;set;}
		public string Queue {get;set;}
		public bool Passive {get;set;}
		public bool Durable {get;set;}
		public bool Exclusive {get;set;}
		public bool AutoDelete {get;set;}
		public bool NoWait {get;set;}
		public Dictionary<string, object> Arguments {get;set;}

		public byte[] Serialize() {
			var writer = new BinWriter();
			writer.WriteShort(SourceClassId);
			writer.WriteShort(SourceMethodId);
			writer.WriteShort(Reserved1);
			writer.WriteShortStr(Queue);
			writer.WriteBit((byte)(Passive ? 1 : 0), false);
			writer.WriteBit((byte)(Durable ? 2 : 0), true);
			writer.WriteBit((byte)(Exclusive ? 4 : 0), true);
			writer.WriteBit((byte)(AutoDelete ? 8 : 0), true);
			writer.WriteBit((byte)(NoWait ? 16 : 0), true);
			writer.WriteFieldTable(Arguments);
			return writer.ToArray();
		}

		public void Deserialize(byte[] bytes) {
			var reader = new BinReader(bytes);
			reader.ReadShort();
			reader.ReadShort();
			Reserved1 = reader.ReadShort();
			Queue = reader.ReadShortStr();
			var flags = reader.ReadByte();
			Passive = (flags & 1) > 0;
			Durable = (flags & 2) > 0;
			Exclusive = (flags & 4) > 0;
			AutoDelete = (flags & 8) > 0;
			NoWait = (flags & 16) > 0;
			Arguments = reader.ReadFieldTable();
		}
	}

	public class DeclareOk {
		public const short SourceClassId = 50;
		public const short SourceMethodId = 11;
		public const bool IsAsyncResponse = true;
		public const bool HasBody = false;

		public string Queue {get;set;}
		public int MessageCount {get;set;}
		public int ConsumerCount {get;set;}

		public byte[] Serialize() {
			var writer = new BinWriter();
			writer.WriteShort(SourceClassId);
			writer.WriteShort(SourceMethodId);
			writer.WriteShortStr(Queue);
			writer.WriteInt(MessageCount);
			writer.WriteInt(ConsumerCount);
			return writer.ToArray();
		}

		public void Deserialize(byte[] bytes) {
			var reader = new BinReader(bytes);
			reader.ReadShort();
			reader.ReadShort();
			Queue = reader.ReadShortStr();
			MessageCount = reader.ReadInt();
			ConsumerCount = reader.ReadInt();
		}
	}

	public class Bind {
		public const short SourceClassId = 50;
		public const short SourceMethodId = 20;
		public const bool IsAsyncResponse = false;
		public const bool HasBody = false;

		public short Reserved1 {get;set;}
		public string Queue {get;set;}
		public string Exchange {get;set;}
		public string RoutingKey {get;set;}
		public bool NoWait {get;set;}
		public Dictionary<string, object> Arguments {get;set;}

		public byte[] Serialize() {
			var writer = new BinWriter();
			writer.WriteShort(SourceClassId);
			writer.WriteShort(SourceMethodId);
			writer.WriteShort(Reserved1);
			writer.WriteShortStr(Queue);
			writer.WriteShortStr(Exchange);
			writer.WriteShortStr(RoutingKey);
			writer.WriteBit((byte)(NoWait ? 1 : 0), false);
			writer.WriteFieldTable(Arguments);
			return writer.ToArray();
		}

		public void Deserialize(byte[] bytes) {
			var reader = new BinReader(bytes);
			reader.ReadShort();
			reader.ReadShort();
			Reserved1 = reader.ReadShort();
			Queue = reader.ReadShortStr();
			Exchange = reader.ReadShortStr();
			RoutingKey = reader.ReadShortStr();
			var flags = reader.ReadByte();
			NoWait = (flags & 1) > 0;
			Arguments = reader.ReadFieldTable();
		}
	}

	public class BindOk {
		public const short SourceClassId = 50;
		public const short SourceMethodId = 21;
		public const bool IsAsyncResponse = true;
		public const bool HasBody = false;


		public byte[] Serialize() {
			var writer = new BinWriter();
			writer.WriteShort(SourceClassId);
			writer.WriteShort(SourceMethodId);
			return writer.ToArray();
		}

		public void Deserialize(byte[] bytes) {
			var reader = new BinReader(bytes);
			reader.ReadShort();
			reader.ReadShort();
		}
	}

	public class Unbind {
		public const short SourceClassId = 50;
		public const short SourceMethodId = 50;
		public const bool IsAsyncResponse = false;
		public const bool HasBody = false;

		public short Reserved1 {get;set;}
		public string Queue {get;set;}
		public string Exchange {get;set;}
		public string RoutingKey {get;set;}
		public Dictionary<string, object> Arguments {get;set;}

		public byte[] Serialize() {
			var writer = new BinWriter();
			writer.WriteShort(SourceClassId);
			writer.WriteShort(SourceMethodId);
			writer.WriteShort(Reserved1);
			writer.WriteShortStr(Queue);
			writer.WriteShortStr(Exchange);
			writer.WriteShortStr(RoutingKey);
			writer.WriteFieldTable(Arguments);
			return writer.ToArray();
		}

		public void Deserialize(byte[] bytes) {
			var reader = new BinReader(bytes);
			reader.ReadShort();
			reader.ReadShort();
			Reserved1 = reader.ReadShort();
			Queue = reader.ReadShortStr();
			Exchange = reader.ReadShortStr();
			RoutingKey = reader.ReadShortStr();
			Arguments = reader.ReadFieldTable();
		}
	}

	public class UnbindOk {
		public const short SourceClassId = 50;
		public const short SourceMethodId = 51;
		public const bool IsAsyncResponse = true;
		public const bool HasBody = false;


		public byte[] Serialize() {
			var writer = new BinWriter();
			writer.WriteShort(SourceClassId);
			writer.WriteShort(SourceMethodId);
			return writer.ToArray();
		}

		public void Deserialize(byte[] bytes) {
			var reader = new BinReader(bytes);
			reader.ReadShort();
			reader.ReadShort();
		}
	}

	public class Purge {
		public const short SourceClassId = 50;
		public const short SourceMethodId = 30;
		public const bool IsAsyncResponse = false;
		public const bool HasBody = false;

		public short Reserved1 {get;set;}
		public string Queue {get;set;}
		public bool NoWait {get;set;}

		public byte[] Serialize() {
			var writer = new BinWriter();
			writer.WriteShort(SourceClassId);
			writer.WriteShort(SourceMethodId);
			writer.WriteShort(Reserved1);
			writer.WriteShortStr(Queue);
			writer.WriteBit((byte)(NoWait ? 1 : 0), false);
			return writer.ToArray();
		}

		public void Deserialize(byte[] bytes) {
			var reader = new BinReader(bytes);
			reader.ReadShort();
			reader.ReadShort();
			Reserved1 = reader.ReadShort();
			Queue = reader.ReadShortStr();
			var flags = reader.ReadByte();
			NoWait = (flags & 1) > 0;
		}
	}

	public class PurgeOk {
		public const short SourceClassId = 50;
		public const short SourceMethodId = 31;
		public const bool IsAsyncResponse = true;
		public const bool HasBody = false;

		public int MessageCount {get;set;}

		public byte[] Serialize() {
			var writer = new BinWriter();
			writer.WriteShort(SourceClassId);
			writer.WriteShort(SourceMethodId);
			writer.WriteInt(MessageCount);
			return writer.ToArray();
		}

		public void Deserialize(byte[] bytes) {
			var reader = new BinReader(bytes);
			reader.ReadShort();
			reader.ReadShort();
			MessageCount = reader.ReadInt();
		}
	}

	public class Delete {
		public const short SourceClassId = 50;
		public const short SourceMethodId = 40;
		public const bool IsAsyncResponse = false;
		public const bool HasBody = false;

		public short Reserved1 {get;set;}
		public string Queue {get;set;}
		public bool IfUnused {get;set;}
		public bool IfEmpty {get;set;}
		public bool NoWait {get;set;}

		public byte[] Serialize() {
			var writer = new BinWriter();
			writer.WriteShort(SourceClassId);
			writer.WriteShort(SourceMethodId);
			writer.WriteShort(Reserved1);
			writer.WriteShortStr(Queue);
			writer.WriteBit((byte)(IfUnused ? 1 : 0), false);
			writer.WriteBit((byte)(IfEmpty ? 2 : 0), true);
			writer.WriteBit((byte)(NoWait ? 4 : 0), true);
			return writer.ToArray();
		}

		public void Deserialize(byte[] bytes) {
			var reader = new BinReader(bytes);
			reader.ReadShort();
			reader.ReadShort();
			Reserved1 = reader.ReadShort();
			Queue = reader.ReadShortStr();
			var flags = reader.ReadByte();
			IfUnused = (flags & 1) > 0;
			IfEmpty = (flags & 2) > 0;
			NoWait = (flags & 4) > 0;
		}
	}

	public class DeleteOk {
		public const short SourceClassId = 50;
		public const short SourceMethodId = 41;
		public const bool IsAsyncResponse = true;
		public const bool HasBody = false;

		public int MessageCount {get;set;}

		public byte[] Serialize() {
			var writer = new BinWriter();
			writer.WriteShort(SourceClassId);
			writer.WriteShort(SourceMethodId);
			writer.WriteInt(MessageCount);
			return writer.ToArray();
		}

		public void Deserialize(byte[] bytes) {
			var reader = new BinReader(bytes);
			reader.ReadShort();
			reader.ReadShort();
			MessageCount = reader.ReadInt();
		}
	}

}
