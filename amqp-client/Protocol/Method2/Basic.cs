namespace AMQPClient.Protocol.Method2;

public class Basic {
	public class Qos : IFrameMethod {
		private const short _sourceClassId = 60;
		private const short _sourceMethodId = 10;
		public short SourceClassId => _sourceClassId;
		 public short SourceMethodId => _sourceMethodId;
		public const bool IsAsyncResponse = false;
		public const bool HasBody = false;

		public int PrefetchSize { get; set; }
		public short PrefetchCount { get; set; }
		public bool Global { get; set; }

		public byte[] Serialize() {
			var writer = new BinWriter();
			writer.WriteShort(SourceClassId);
			writer.WriteShort(SourceMethodId);
			writer.WriteInt(PrefetchSize);
			writer.WriteShort(PrefetchCount);
			writer.WriteBit((byte)(Global ? 1 : 0), false);
			return writer.ToArray();
		}

		public void Deserialize(byte[] bytes) {
			var reader = new BinReader(bytes);
			reader.ReadShort();
			reader.ReadShort();
			PrefetchSize = reader.ReadInt();
			PrefetchCount = reader.ReadShort();
			var flags = reader.ReadByte();
			Global = (flags & 1) > 0;
		}
	}

	public class QosOk : IFrameMethod {
		private const short _sourceClassId = 60;
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

	public class Consume : IFrameMethod {
		private const short _sourceClassId = 60;
		private const short _sourceMethodId = 20;
		public short SourceClassId => _sourceClassId;
		 public short SourceMethodId => _sourceMethodId;
		public const bool IsAsyncResponse = false;
		public const bool HasBody = false;

		public short Reserved1 { get; set; }
		public string Queue { get; set; }
		public string ConsumerTag { get; set; }
		public bool NoLocal { get; set; }
		public bool NoAck { get; set; }
		public bool Exclusive { get; set; }
		public bool NoWait { get; set; }
		public Dictionary<string, object> Arguments { get; set; }

		public byte[] Serialize() {
			var writer = new BinWriter();
			writer.WriteShort(SourceClassId);
			writer.WriteShort(SourceMethodId);
			writer.WriteShort(Reserved1);
			writer.WriteShortStr(Queue);
			writer.WriteShortStr(ConsumerTag);
			writer.WriteBit((byte)(NoLocal ? 1 : 0), false);
			writer.WriteBit((byte)(NoAck ? 2 : 0), true);
			writer.WriteBit((byte)(Exclusive ? 4 : 0), true);
			writer.WriteBit((byte)(NoWait ? 8 : 0), true);
			writer.WriteFieldTable(Arguments);
			return writer.ToArray();
		}

		public void Deserialize(byte[] bytes) {
			var reader = new BinReader(bytes);
			reader.ReadShort();
			reader.ReadShort();
			Reserved1 = reader.ReadShort();
			Queue = reader.ReadShortStr();
			ConsumerTag = reader.ReadShortStr();
			var flags = reader.ReadByte();
			NoLocal = (flags & 1) > 0;
			NoAck = (flags & 2) > 0;
			Exclusive = (flags & 4) > 0;
			NoWait = (flags & 8) > 0;
			Arguments = reader.ReadFieldTable();
		}
	}

	public class ConsumeOk : IFrameMethod {
		private const short _sourceClassId = 60;
		private const short _sourceMethodId = 21;
		public short SourceClassId => _sourceClassId;
		 public short SourceMethodId => _sourceMethodId;
		public const bool IsAsyncResponse = true;
		public const bool HasBody = false;

		public string ConsumerTag { get; set; }

		public byte[] Serialize() {
			var writer = new BinWriter();
			writer.WriteShort(SourceClassId);
			writer.WriteShort(SourceMethodId);
			writer.WriteShortStr(ConsumerTag);
			return writer.ToArray();
		}

		public void Deserialize(byte[] bytes) {
			var reader = new BinReader(bytes);
			reader.ReadShort();
			reader.ReadShort();
			ConsumerTag = reader.ReadShortStr();
		}
	}

	public class Cancel : IFrameMethod {
		private const short _sourceClassId = 60;
		private const short _sourceMethodId = 30;
		public short SourceClassId => _sourceClassId;
		 public short SourceMethodId => _sourceMethodId;
		public const bool IsAsyncResponse = false;
		public const bool HasBody = false;

		public string ConsumerTag { get; set; }
		public bool NoWait { get; set; }

		public byte[] Serialize() {
			var writer = new BinWriter();
			writer.WriteShort(SourceClassId);
			writer.WriteShort(SourceMethodId);
			writer.WriteShortStr(ConsumerTag);
			writer.WriteBit((byte)(NoWait ? 1 : 0), false);
			return writer.ToArray();
		}

		public void Deserialize(byte[] bytes) {
			var reader = new BinReader(bytes);
			reader.ReadShort();
			reader.ReadShort();
			ConsumerTag = reader.ReadShortStr();
			var flags = reader.ReadByte();
			NoWait = (flags & 1) > 0;
		}
	}

	public class CancelOk : IFrameMethod {
		private const short _sourceClassId = 60;
		private const short _sourceMethodId = 31;
		public short SourceClassId => _sourceClassId;
		 public short SourceMethodId => _sourceMethodId;
		public const bool IsAsyncResponse = true;
		public const bool HasBody = false;

		public string ConsumerTag { get; set; }

		public byte[] Serialize() {
			var writer = new BinWriter();
			writer.WriteShort(SourceClassId);
			writer.WriteShort(SourceMethodId);
			writer.WriteShortStr(ConsumerTag);
			return writer.ToArray();
		}

		public void Deserialize(byte[] bytes) {
			var reader = new BinReader(bytes);
			reader.ReadShort();
			reader.ReadShort();
			ConsumerTag = reader.ReadShortStr();
		}
	}

	public class Publish : IFrameMethod {
		private const short _sourceClassId = 60;
		private const short _sourceMethodId = 40;
		public short SourceClassId => _sourceClassId;
		 public short SourceMethodId => _sourceMethodId;
		public const bool IsAsyncResponse = false;
		public const bool HasBody = false;

		public short Reserved1 { get; set; }
		public string Exchange { get; set; }
		public string RoutingKey { get; set; }
		public bool Mandatory { get; set; }
		public bool Immediate { get; set; }

		public byte[] Serialize() {
			var writer = new BinWriter();
			writer.WriteShort(SourceClassId);
			writer.WriteShort(SourceMethodId);
			writer.WriteShort(Reserved1);
			writer.WriteShortStr(Exchange);
			writer.WriteShortStr(RoutingKey);
			writer.WriteBit((byte)(Mandatory ? 1 : 0), false);
			writer.WriteBit((byte)(Immediate ? 2 : 0), true);
			return writer.ToArray();
		}

		public void Deserialize(byte[] bytes) {
			var reader = new BinReader(bytes);
			reader.ReadShort();
			reader.ReadShort();
			Reserved1 = reader.ReadShort();
			Exchange = reader.ReadShortStr();
			RoutingKey = reader.ReadShortStr();
			var flags = reader.ReadByte();
			Mandatory = (flags & 1) > 0;
			Immediate = (flags & 2) > 0;
		}
	}

	public class Return : IFrameMethod {
		private const short _sourceClassId = 60;
		private const short _sourceMethodId = 50;
		public short SourceClassId => _sourceClassId;
		 public short SourceMethodId => _sourceMethodId;
		public const bool IsAsyncResponse = false;
		public const bool HasBody = false;

		public short ReplyCode { get; set; }
		public string ReplyText { get; set; }
		public string Exchange { get; set; }
		public string RoutingKey { get; set; }

		public byte[] Serialize() {
			var writer = new BinWriter();
			writer.WriteShort(SourceClassId);
			writer.WriteShort(SourceMethodId);
			writer.WriteShort(ReplyCode);
			writer.WriteShortStr(ReplyText);
			writer.WriteShortStr(Exchange);
			writer.WriteShortStr(RoutingKey);
			return writer.ToArray();
		}

		public void Deserialize(byte[] bytes) {
			var reader = new BinReader(bytes);
			reader.ReadShort();
			reader.ReadShort();
			ReplyCode = reader.ReadShort();
			ReplyText = reader.ReadShortStr();
			Exchange = reader.ReadShortStr();
			RoutingKey = reader.ReadShortStr();
		}
	}

	public class Deliver : IFrameMethod {
		private const short _sourceClassId = 60;
		private const short _sourceMethodId = 60;
		public short SourceClassId => _sourceClassId;
		 public short SourceMethodId => _sourceMethodId;
		public const bool IsAsyncResponse = false;
		public const bool HasBody = false;

		public string ConsumerTag { get; set; }
		public int DeliveryTag { get; set; }
		public bool Redelivered { get; set; }
		public string Exchange { get; set; }
		public string RoutingKey { get; set; }

		public byte[] Serialize() {
			var writer = new BinWriter();
			writer.WriteShort(SourceClassId);
			writer.WriteShort(SourceMethodId);
			writer.WriteShortStr(ConsumerTag);
			writer.WriteInt(DeliveryTag);
			writer.WriteBit((byte)(Redelivered ? 1 : 0), false);
			writer.WriteShortStr(Exchange);
			writer.WriteShortStr(RoutingKey);
			return writer.ToArray();
		}

		public void Deserialize(byte[] bytes) {
			var reader = new BinReader(bytes);
			reader.ReadShort();
			reader.ReadShort();
			ConsumerTag = reader.ReadShortStr();
			DeliveryTag = reader.ReadInt();
			var flags = reader.ReadByte();
			Redelivered = (flags & 1) > 0;
			Exchange = reader.ReadShortStr();
			RoutingKey = reader.ReadShortStr();
		}
	}

	public class Get : IFrameMethod {
		private const short _sourceClassId = 60;
		private const short _sourceMethodId = 70;
		public short SourceClassId => _sourceClassId;
		 public short SourceMethodId => _sourceMethodId;
		public const bool IsAsyncResponse = false;
		public const bool HasBody = false;

		public short Reserved1 { get; set; }
		public string Queue { get; set; }
		public bool NoAck { get; set; }

		public byte[] Serialize() {
			var writer = new BinWriter();
			writer.WriteShort(SourceClassId);
			writer.WriteShort(SourceMethodId);
			writer.WriteShort(Reserved1);
			writer.WriteShortStr(Queue);
			writer.WriteBit((byte)(NoAck ? 1 : 0), false);
			return writer.ToArray();
		}

		public void Deserialize(byte[] bytes) {
			var reader = new BinReader(bytes);
			reader.ReadShort();
			reader.ReadShort();
			Reserved1 = reader.ReadShort();
			Queue = reader.ReadShortStr();
			var flags = reader.ReadByte();
			NoAck = (flags & 1) > 0;
		}
	}

	public class GetOk : IFrameMethod {
		private const short _sourceClassId = 60;
		private const short _sourceMethodId = 71;
		public short SourceClassId => _sourceClassId;
		 public short SourceMethodId => _sourceMethodId;
		public const bool IsAsyncResponse = true;
		public const bool HasBody = false;

		public int DeliveryTag { get; set; }
		public bool Redelivered { get; set; }
		public string Exchange { get; set; }
		public string RoutingKey { get; set; }
		public int MessageCount { get; set; }

		public byte[] Serialize() {
			var writer = new BinWriter();
			writer.WriteShort(SourceClassId);
			writer.WriteShort(SourceMethodId);
			writer.WriteInt(DeliveryTag);
			writer.WriteBit((byte)(Redelivered ? 1 : 0), false);
			writer.WriteShortStr(Exchange);
			writer.WriteShortStr(RoutingKey);
			writer.WriteInt(MessageCount);
			return writer.ToArray();
		}

		public void Deserialize(byte[] bytes) {
			var reader = new BinReader(bytes);
			reader.ReadShort();
			reader.ReadShort();
			DeliveryTag = reader.ReadInt();
			var flags = reader.ReadByte();
			Redelivered = (flags & 1) > 0;
			Exchange = reader.ReadShortStr();
			RoutingKey = reader.ReadShortStr();
			MessageCount = reader.ReadInt();
		}
	}

	public class GetEmpty : IFrameMethod {
		private const short _sourceClassId = 60;
		private const short _sourceMethodId = 72;
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

	public class Ack : IFrameMethod {
		private const short _sourceClassId = 60;
		private const short _sourceMethodId = 80;
		public short SourceClassId => _sourceClassId;
		 public short SourceMethodId => _sourceMethodId;
		public const bool IsAsyncResponse = false;
		public const bool HasBody = false;

		public int DeliveryTag { get; set; }
		public bool Multiple { get; set; }

		public byte[] Serialize() {
			var writer = new BinWriter();
			writer.WriteShort(SourceClassId);
			writer.WriteShort(SourceMethodId);
			writer.WriteInt(DeliveryTag);
			writer.WriteBit((byte)(Multiple ? 1 : 0), false);
			return writer.ToArray();
		}

		public void Deserialize(byte[] bytes) {
			var reader = new BinReader(bytes);
			reader.ReadShort();
			reader.ReadShort();
			DeliveryTag = reader.ReadInt();
			var flags = reader.ReadByte();
			Multiple = (flags & 1) > 0;
		}
	}

	public class Reject : IFrameMethod {
		private const short _sourceClassId = 60;
		private const short _sourceMethodId = 90;
		public short SourceClassId => _sourceClassId;
		 public short SourceMethodId => _sourceMethodId;
		public const bool IsAsyncResponse = false;
		public const bool HasBody = false;

		public int DeliveryTag { get; set; }
		public bool Requeue { get; set; }

		public byte[] Serialize() {
			var writer = new BinWriter();
			writer.WriteShort(SourceClassId);
			writer.WriteShort(SourceMethodId);
			writer.WriteInt(DeliveryTag);
			writer.WriteBit((byte)(Requeue ? 1 : 0), false);
			return writer.ToArray();
		}

		public void Deserialize(byte[] bytes) {
			var reader = new BinReader(bytes);
			reader.ReadShort();
			reader.ReadShort();
			DeliveryTag = reader.ReadInt();
			var flags = reader.ReadByte();
			Requeue = (flags & 1) > 0;
		}
	}

	public class RecoverAsync : IFrameMethod {
		private const short _sourceClassId = 60;
		private const short _sourceMethodId = 100;
		public short SourceClassId => _sourceClassId;
		 public short SourceMethodId => _sourceMethodId;
		public const bool IsAsyncResponse = false;
		public const bool HasBody = false;

		public bool Requeue { get; set; }

		public byte[] Serialize() {
			var writer = new BinWriter();
			writer.WriteShort(SourceClassId);
			writer.WriteShort(SourceMethodId);
			writer.WriteBit((byte)(Requeue ? 1 : 0), false);
			return writer.ToArray();
		}

		public void Deserialize(byte[] bytes) {
			var reader = new BinReader(bytes);
			reader.ReadShort();
			reader.ReadShort();
			var flags = reader.ReadByte();
			Requeue = (flags & 1) > 0;
		}
	}

	public class Recover : IFrameMethod {
		private const short _sourceClassId = 60;
		private const short _sourceMethodId = 110;
		public short SourceClassId => _sourceClassId;
		 public short SourceMethodId => _sourceMethodId;
		public const bool IsAsyncResponse = false;
		public const bool HasBody = false;

		public bool Requeue { get; set; }

		public byte[] Serialize() {
			var writer = new BinWriter();
			writer.WriteShort(SourceClassId);
			writer.WriteShort(SourceMethodId);
			writer.WriteBit((byte)(Requeue ? 1 : 0), false);
			return writer.ToArray();
		}

		public void Deserialize(byte[] bytes) {
			var reader = new BinReader(bytes);
			reader.ReadShort();
			reader.ReadShort();
			var flags = reader.ReadByte();
			Requeue = (flags & 1) > 0;
		}
	}

	public class RecoverOk : IFrameMethod {
		private const short _sourceClassId = 60;
		private const short _sourceMethodId = 111;
		public short SourceClassId => _sourceClassId;
		 public short SourceMethodId => _sourceMethodId;
		public const bool IsAsyncResponse = false;
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
