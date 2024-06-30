namespace AMQPClient.Protocol.Method2;

public class Queue {
	public class Declare : IFrameMethod {
		public short SourceClassId => 50;
		public short SourceMethodId => 10;
		public bool IsAsyncResponse => false;
		public bool HasBody => false;

		public short Reserved1 { get; set; }
		public string Queue { get; set; }
		public bool Passive { get; set; }
		public bool Durable { get; set; }
		public bool Exclusive { get; set; }
		public bool AutoDelete { get; set; }
		public bool NoWait { get; set; }
		public Dictionary<string, object> Arguments { get; set; }

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

	public class DeclareOk : IFrameMethod {
		public short SourceClassId => 50;
		public short SourceMethodId => 11;
		public bool IsAsyncResponse => true;
		public bool HasBody => false;

		public string Queue { get; set; }
		public int MessageCount { get; set; }
		public int ConsumerCount { get; set; }

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

	public class Bind : IFrameMethod {
		public short SourceClassId => 50;
		public short SourceMethodId => 20;
		public bool IsAsyncResponse => false;
		public bool HasBody => false;

		public short Reserved1 { get; set; }
		public string Queue { get; set; }
		public string Exchange { get; set; }
		public string RoutingKey { get; set; }
		public bool NoWait { get; set; }
		public Dictionary<string, object> Arguments { get; set; }

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

	public class BindOk : IFrameMethod {
		public short SourceClassId => 50;
		public short SourceMethodId => 21;
		public bool IsAsyncResponse => true;
		public bool HasBody => false;

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

	public class Unbind : IFrameMethod {
		public short SourceClassId => 50;
		public short SourceMethodId => 50;
		public bool IsAsyncResponse => false;
		public bool HasBody => false;

		public short Reserved1 { get; set; }
		public string Queue { get; set; }
		public string Exchange { get; set; }
		public string RoutingKey { get; set; }
		public Dictionary<string, object> Arguments { get; set; }

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

	public class UnbindOk : IFrameMethod {
		public short SourceClassId => 50;
		public short SourceMethodId => 51;
		public bool IsAsyncResponse => true;
		public bool HasBody => false;

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

	public class Purge : IFrameMethod {
		public short SourceClassId => 50;
		public short SourceMethodId => 30;
		public bool IsAsyncResponse => false;
		public bool HasBody => false;

		public short Reserved1 { get; set; }
		public string Queue { get; set; }
		public bool NoWait { get; set; }

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

	public class PurgeOk : IFrameMethod {
		public short SourceClassId => 50;
		public short SourceMethodId => 31;
		public bool IsAsyncResponse => true;
		public bool HasBody => false;

		public int MessageCount { get; set; }

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

	public class Delete : IFrameMethod {
		public short SourceClassId => 50;
		public short SourceMethodId => 40;
		public bool IsAsyncResponse => false;
		public bool HasBody => false;

		public short Reserved1 { get; set; }
		public string Queue { get; set; }
		public bool IfUnused { get; set; }
		public bool IfEmpty { get; set; }
		public bool NoWait { get; set; }

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

	public class DeleteOk : IFrameMethod {
		public short SourceClassId => 50;
		public short SourceMethodId => 41;
		public bool IsAsyncResponse => true;
		public bool HasBody => false;

		public int MessageCount { get; set; }

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
