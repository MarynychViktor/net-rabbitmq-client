using AMQPClient.Protocol;

public class Channel {
	public class Open {
		public const short SourceClassId = 20;
		public const short SourceMethodId = 10;
		public const bool IsAsyncResponse = false;
		public const bool HasBody = false;

		public string Reserved1 { get; set; }

		public byte[] Serialize() {
			var writer = new BinWriter();
			writer.WriteShort(SourceClassId);
			writer.WriteShort(SourceMethodId);
			writer.WriteShortStr(Reserved1);
			return writer.ToArray();
		}

		public void Deserialize(byte[] bytes) {
			var reader = new BinReader(bytes);
			reader.ReadShort();
			reader.ReadShort();
			Reserved1 = reader.ReadShortStr();
		}
	}

	public class OpenOk {
		public const short SourceClassId = 20;
		public const short SourceMethodId = 11;
		public const bool IsAsyncResponse = true;
		public const bool HasBody = false;

		public string Reserved1 { get; set; }

		public byte[] Serialize() {
			var writer = new BinWriter();
			writer.WriteShort(SourceClassId);
			writer.WriteShort(SourceMethodId);
			writer.WriteLongStr(Reserved1);
			return writer.ToArray();
		}

		public void Deserialize(byte[] bytes) {
			var reader = new BinReader(bytes);
			reader.ReadShort();
			reader.ReadShort();
			Reserved1 = reader.ReadLongStr();
		}
	}

	public class Flow {
		public const short SourceClassId = 20;
		public const short SourceMethodId = 20;
		public const bool IsAsyncResponse = false;
		public const bool HasBody = false;

		public bool Active { get; set; }

		public byte[] Serialize() {
			var writer = new BinWriter();
			writer.WriteShort(SourceClassId);
			writer.WriteShort(SourceMethodId);
			writer.WriteBit((byte)(Active ? 1 : 0), false);
			return writer.ToArray();
		}

		public void Deserialize(byte[] bytes) {
			var reader = new BinReader(bytes);
			reader.ReadShort();
			reader.ReadShort();
			var flags = reader.ReadByte();
			Active = (flags & 1) > 0;
		}
	}

	public class FlowOk {
		public const short SourceClassId = 20;
		public const short SourceMethodId = 21;
		public const bool IsAsyncResponse = true;
		public const bool HasBody = false;

		public bool Active { get; set; }

		public byte[] Serialize() {
			var writer = new BinWriter();
			writer.WriteShort(SourceClassId);
			writer.WriteShort(SourceMethodId);
			writer.WriteBit((byte)(Active ? 1 : 0), false);
			return writer.ToArray();
		}

		public void Deserialize(byte[] bytes) {
			var reader = new BinReader(bytes);
			reader.ReadShort();
			reader.ReadShort();
			var flags = reader.ReadByte();
			Active = (flags & 1) > 0;
		}
	}

	public class Close {
		public const short SourceClassId = 20;
		public const short SourceMethodId = 40;
		public const bool IsAsyncResponse = false;
		public const bool HasBody = false;

		public short ReplyCode { get; set; }
		public string ReplyText { get; set; }
		public short ClassId { get; set; }
		public short MethodId { get; set; }

		public byte[] Serialize() {
			var writer = new BinWriter();
			writer.WriteShort(SourceClassId);
			writer.WriteShort(SourceMethodId);
			writer.WriteShort(ReplyCode);
			writer.WriteShortStr(ReplyText);
			writer.WriteShort(ClassId);
			writer.WriteShort(MethodId);
			return writer.ToArray();
		}

		public void Deserialize(byte[] bytes) {
			var reader = new BinReader(bytes);
			reader.ReadShort();
			reader.ReadShort();
			ReplyCode = reader.ReadShort();
			ReplyText = reader.ReadShortStr();
			ClassId = reader.ReadShort();
			MethodId = reader.ReadShort();
		}
	}

	public class CloseOk {
		public const short SourceClassId = 20;
		public const short SourceMethodId = 41;
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
