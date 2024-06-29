using AMQPClient.Protocol;

public class Basic {
	public class Qos {
		public const short SourceClassId = 60;
		public const short SourceMethodId = 10;
		public const bool IsAsyncResponse = false;
		public const bool HasBody = false;

		public int PrefetchSize {get;set;}
		public short PrefetchCount {get;set;}
		public bool Global {get;set;}

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

	public class QosOk {
		public const short SourceClassId = 60;
		public const short SourceMethodId = 11;
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

	public class Consume {
		public const short SourceClassId = 60;
		public const short SourceMethodId = 20;
		public const bool IsAsyncResponse = false;
		public const bool HasBody = false;

		public short Reserved1 {get;set;}
		public string Queue {get;set;}
		public string ConsumerTag {get;set;}
		public bool NoLocal {get;set;}
		public bool NoAck {get;set;}
		public bool Exclusive {get;set;}
		public bool NoWait {get;set;}
		public Dictionary<string, object> Arguments {get;set;}

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

	public class ConsumeOk {
		public const short SourceClassId = 60;
		public const short SourceMethodId = 21;
		public const bool IsAsyncResponse = true;
		public const bool HasBody = false;

		public string ConsumerTag {get;set;}

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

	public class Cancel {
		public const short SourceClassId = 60;
		public const short SourceMethodId = 30;
		public const bool IsAsyncResponse = false;
		public const bool HasBody = false;

		public string ConsumerTag {get;set;}
		public bool NoWait {get;set;}

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

	public class CancelOk {
		public const short SourceClassId = 60;
		public const short SourceMethodId = 31;
		public const bool IsAsyncResponse = true;
		public const bool HasBody = false;

		public string ConsumerTag {get;set;}

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

	public class Publish {
		public const short SourceClassId = 60;
		public const short SourceMethodId = 40;
		public const bool IsAsyncResponse = false;
		public const bool HasBody = false;

		public short Reserved1 {get;set;}
		public string Exchange {get;set;}
		public string RoutingKey {get;set;}
		public bool Mandatory {get;set;}
		public bool Immediate {get;set;}

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

	public class Return {
		public const short SourceClassId = 60;
		public const short SourceMethodId = 50;
		public const bool IsAsyncResponse = false;
		public const bool HasBody = false;

		public short ReplyCode {get;set;}
		public string ReplyText {get;set;}
		public string Exchange {get;set;}
		public string RoutingKey {get;set;}

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

	public class Deliver {
		public const short SourceClassId = 60;
		public const short SourceMethodId = 60;
		public const bool IsAsyncResponse = false;
		public const bool HasBody = false;

		public string ConsumerTag {get;set;}
		public int DeliveryTag {get;set;}
		public bool Redelivered {get;set;}
		public string Exchange {get;set;}
		public string RoutingKey {get;set;}

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

	public class Get {
		public const short SourceClassId = 60;
		public const short SourceMethodId = 70;
		public const bool IsAsyncResponse = false;
		public const bool HasBody = false;

		public short Reserved1 {get;set;}
		public string Queue {get;set;}
		public bool NoAck {get;set;}

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

	public class GetOk {
		public const short SourceClassId = 60;
		public const short SourceMethodId = 71;
		public const bool IsAsyncResponse = true;
		public const bool HasBody = false;

		public int DeliveryTag {get;set;}
		public bool Redelivered {get;set;}
		public string Exchange {get;set;}
		public string RoutingKey {get;set;}
		public int MessageCount {get;set;}

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

	public class GetEmpty {
		public const short SourceClassId = 60;
		public const short SourceMethodId = 72;
		public const bool IsAsyncResponse = false;
		public const bool HasBody = false;

		public string Reserved1 {get;set;}

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

	public class Ack {
		public const short SourceClassId = 60;
		public const short SourceMethodId = 80;
		public const bool IsAsyncResponse = false;
		public const bool HasBody = false;

		public int DeliveryTag {get;set;}
		public bool Multiple {get;set;}

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

	public class Reject {
		public const short SourceClassId = 60;
		public const short SourceMethodId = 90;
		public const bool IsAsyncResponse = false;
		public const bool HasBody = false;

		public int DeliveryTag {get;set;}
		public bool Requeue {get;set;}

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

	public class RecoverAsync {
		public const short SourceClassId = 60;
		public const short SourceMethodId = 100;
		public const bool IsAsyncResponse = false;
		public const bool HasBody = false;

		public bool Requeue {get;set;}

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

	public class Recover {
		public const short SourceClassId = 60;
		public const short SourceMethodId = 110;
		public const bool IsAsyncResponse = false;
		public const bool HasBody = false;

		public bool Requeue {get;set;}

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

	public class RecoverOk {
		public const short SourceClassId = 60;
		public const short SourceMethodId = 111;
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
