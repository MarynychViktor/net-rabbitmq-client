namespace AMQPClient.Protocol.Method2;

public class Exchange {
	public class Declare : IFrameMethod {
		private const short _sourceClassId = 40;
		private const short _sourceMethodId = 10;
		public short SourceClassId => _sourceClassId;
		 public short SourceMethodId => _sourceMethodId;
		public const bool IsAsyncResponse = false;
		public const bool HasBody = false;

		public short Reserved1 { get; set; }
		public string Exchange { get; set; }
		public string Type { get; set; }
		public bool Passive { get; set; }
		public bool Durable { get; set; }
		public bool Reserved2 { get; set; }
		public bool Reserved3 { get; set; }
		public bool NoWait { get; set; }
		public Dictionary<string, object> Arguments { get; set; }

		public byte[] Serialize() {
			var writer = new BinWriter();
			writer.WriteShort(SourceClassId);
			writer.WriteShort(SourceMethodId);
			writer.WriteShort(Reserved1);
			writer.WriteShortStr(Exchange);
			writer.WriteShortStr(Type);
			writer.WriteBit((byte)(Passive ? 1 : 0), false);
			writer.WriteBit((byte)(Durable ? 2 : 0), true);
			writer.WriteBit((byte)(Reserved2 ? 4 : 0), true);
			writer.WriteBit((byte)(Reserved3 ? 8 : 0), true);
			writer.WriteBit((byte)(NoWait ? 16 : 0), true);
			writer.WriteFieldTable(Arguments);
			return writer.ToArray();
		}

		public void Deserialize(byte[] bytes) {
			var reader = new BinReader(bytes);
			reader.ReadShort();
			reader.ReadShort();
			Reserved1 = reader.ReadShort();
			Exchange = reader.ReadShortStr();
			Type = reader.ReadShortStr();
			var flags = reader.ReadByte();
			Passive = (flags & 1) > 0;
			Durable = (flags & 2) > 0;
			Reserved2 = (flags & 4) > 0;
			Reserved3 = (flags & 8) > 0;
			NoWait = (flags & 16) > 0;
			Arguments = reader.ReadFieldTable();
		}
	}

	public class DeclareOk : IFrameMethod {
		private const short _sourceClassId = 40;
		private const short _sourceMethodId = 11;
		public short SourceClassId => _sourceClassId;
		 public short SourceMethodId => _sourceMethodId;
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

	public class Delete : IFrameMethod {
		private const short _sourceClassId = 40;
		private const short _sourceMethodId = 20;
		public short SourceClassId => _sourceClassId;
		 public short SourceMethodId => _sourceMethodId;
		public const bool IsAsyncResponse = false;
		public const bool HasBody = false;

		public short Reserved1 { get; set; }
		public string Exchange { get; set; }
		public bool IfUnused { get; set; }
		public bool NoWait { get; set; }

		public byte[] Serialize() {
			var writer = new BinWriter();
			writer.WriteShort(SourceClassId);
			writer.WriteShort(SourceMethodId);
			writer.WriteShort(Reserved1);
			writer.WriteShortStr(Exchange);
			writer.WriteBit((byte)(IfUnused ? 1 : 0), false);
			writer.WriteBit((byte)(NoWait ? 2 : 0), true);
			return writer.ToArray();
		}

		public void Deserialize(byte[] bytes) {
			var reader = new BinReader(bytes);
			reader.ReadShort();
			reader.ReadShort();
			Reserved1 = reader.ReadShort();
			Exchange = reader.ReadShortStr();
			var flags = reader.ReadByte();
			IfUnused = (flags & 1) > 0;
			NoWait = (flags & 2) > 0;
		}
	}

	public class DeleteOk : IFrameMethod {
		private const short _sourceClassId = 40;
		private const short _sourceMethodId = 21;
		public short SourceClassId => _sourceClassId;
		 public short SourceMethodId => _sourceMethodId;
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

}
