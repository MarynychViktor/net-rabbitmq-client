namespace AMQPClient.Protocol.Method2;

public class Basic {
	public class Qos : IFrameMethod {
		public short SourceClassId => 60;
		public short SourceMethodId => 10;
		public bool IsAsyncResponse => false;
		public bool HasBody => false;

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
		public short SourceClassId => 60;
		public short SourceMethodId => 11;
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

	public class Consume : IFrameMethod {
		public short SourceClassId => 60;
		public short SourceMethodId => 20;
		public bool IsAsyncResponse => false;
		public bool HasBody => false;

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
		public short SourceClassId => 60;
		public short SourceMethodId => 21;
		public bool IsAsyncResponse => true;
		public bool HasBody => false;

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
		public short SourceClassId => 60;
		public short SourceMethodId => 30;
		public bool IsAsyncResponse => false;
		public bool HasBody => false;

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
		public short SourceClassId => 60;
		public short SourceMethodId => 31;
		public bool IsAsyncResponse => true;
		public bool HasBody => false;

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
		public short SourceClassId => 60;
		public short SourceMethodId => 40;
		public bool IsAsyncResponse => false;
		public bool HasBody => true;

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
		public short SourceClassId => 60;
		public short SourceMethodId => 50;
		public bool IsAsyncResponse => false;
		public bool HasBody => false;

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
		public short SourceClassId => 60;
		public short SourceMethodId => 60;
		public bool IsAsyncResponse => false;
		public bool HasBody => true;

		public string ConsumerTag { get; set; }
		public long DeliveryTag { get; set; }
		public bool Redelivered { get; set; }
		public string Exchange { get; set; }
		public string RoutingKey { get; set; }

		public byte[] Serialize() {
			var writer = new BinWriter();
			writer.WriteShort(SourceClassId);
			writer.WriteShort(SourceMethodId);
			writer.WriteShortStr(ConsumerTag);
			writer.WriteLong(DeliveryTag);
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
			DeliveryTag = reader.ReadLong();
			var flags = reader.ReadByte();
			Redelivered = (flags & 1) > 0;
			Exchange = reader.ReadShortStr();
			RoutingKey = reader.ReadShortStr();
		}
	}

	public class Get : IFrameMethod {
		public short SourceClassId => 60;
		public short SourceMethodId => 70;
		public bool IsAsyncResponse => false;
		public bool HasBody => false;

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
		public short SourceClassId => 60;
		public short SourceMethodId => 71;
		public bool IsAsyncResponse => true;
		public bool HasBody => true;

		public long DeliveryTag { get; set; }
		public bool Redelivered { get; set; }
		public string Exchange { get; set; }
		public string RoutingKey { get; set; }
		public int MessageCount { get; set; }

		public byte[] Serialize() {
			var writer = new BinWriter();
			writer.WriteShort(SourceClassId);
			writer.WriteShort(SourceMethodId);
			writer.WriteLong(DeliveryTag);
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
			DeliveryTag = reader.ReadLong();
			var flags = reader.ReadByte();
			Redelivered = (flags & 1) > 0;
			Exchange = reader.ReadShortStr();
			RoutingKey = reader.ReadShortStr();
			MessageCount = reader.ReadInt();
		}
	}

	public class GetEmpty : IFrameMethod {
		public short SourceClassId => 60;
		public short SourceMethodId => 72;
		public bool IsAsyncResponse => false;
		public bool HasBody => false;

		public string Reserved1 { get; set; }= "";

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
		public short SourceClassId => 60;
		public short SourceMethodId => 80;
		public bool IsAsyncResponse => false;
		public bool HasBody => false;

		public long DeliveryTag { get; set; }
		public bool Multiple { get; set; }

		public byte[] Serialize() {
			var writer = new BinWriter();
			writer.WriteShort(SourceClassId);
			writer.WriteShort(SourceMethodId);
			writer.WriteLong(DeliveryTag);
			writer.WriteBit((byte)(Multiple ? 1 : 0), false);
			return writer.ToArray();
		}

		public void Deserialize(byte[] bytes) {
			var reader = new BinReader(bytes);
			reader.ReadShort();
			reader.ReadShort();
			DeliveryTag = reader.ReadLong();
			var flags = reader.ReadByte();
			Multiple = (flags & 1) > 0;
		}
	}

	public class Reject : IFrameMethod {
		public short SourceClassId => 60;
		public short SourceMethodId => 90;
		public bool IsAsyncResponse => false;
		public bool HasBody => false;

		public long DeliveryTag { get; set; }
		public bool Requeue { get; set; }

		public byte[] Serialize() {
			var writer = new BinWriter();
			writer.WriteShort(SourceClassId);
			writer.WriteShort(SourceMethodId);
			writer.WriteLong(DeliveryTag);
			writer.WriteBit((byte)(Requeue ? 1 : 0), false);
			return writer.ToArray();
		}

		public void Deserialize(byte[] bytes) {
			var reader = new BinReader(bytes);
			reader.ReadShort();
			reader.ReadShort();
			DeliveryTag = reader.ReadLong();
			var flags = reader.ReadByte();
			Requeue = (flags & 1) > 0;
		}
	}

	public class RecoverAsync : IFrameMethod {
		public short SourceClassId => 60;
		public short SourceMethodId => 100;
		public bool IsAsyncResponse => false;
		public bool HasBody => false;

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
		public short SourceClassId => 60;
		public short SourceMethodId => 110;
		public bool IsAsyncResponse => false;
		public bool HasBody => false;

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
		public short SourceClassId => 60;
		public short SourceMethodId => 111;
		public bool IsAsyncResponse => false;
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

}
