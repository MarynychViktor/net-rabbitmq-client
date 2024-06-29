namespace AMQPClient.Protocol.Method2;

public class Channel {
	public class Open : IFrameMethod {
		private const short _sourceClassId = 20;
		private const short _sourceMethodId = 10;
		public short SourceClassId => _sourceClassId;
		 public short SourceMethodId => _sourceMethodId;
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

	public class OpenOk : IFrameMethod {
		private const short _sourceClassId = 20;
		private const short _sourceMethodId = 11;
		public short SourceClassId => _sourceClassId;
		 public short SourceMethodId => _sourceMethodId;
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

	public class Flow : IFrameMethod {
		private const short _sourceClassId = 20;
		private const short _sourceMethodId = 20;
		public short SourceClassId => _sourceClassId;
		 public short SourceMethodId => _sourceMethodId;
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

	public class FlowOk : IFrameMethod {
		private const short _sourceClassId = 20;
		private const short _sourceMethodId = 21;
		public short SourceClassId => _sourceClassId;
		 public short SourceMethodId => _sourceMethodId;
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

	public class Close : IFrameMethod {
		private const short _sourceClassId = 20;
		private const short _sourceMethodId = 40;
		public short SourceClassId => _sourceClassId;
		 public short SourceMethodId => _sourceMethodId;
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

	public class CloseOk : IFrameMethod {
		private const short _sourceClassId = 20;
		private const short _sourceMethodId = 41;
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
