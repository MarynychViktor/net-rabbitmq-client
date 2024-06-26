namespace AMQPClient.Protocol.Classes;

/// <summary>
/// Generated by AMQPProtocolGenerators application. Class represents "exchange" from AMQP protocol.
/// Version 1719733928798
/// <see href="..\AMQPProtocolGenerators\AMQPProtocolGenerators.csproj" />
/// </summary>
public static class Exchange {
	/// <summary>
	/// Generated by AMQPProtocolGenerators application. Method represents "declare" from AMQP protocol.
	/// <see href="..\AMQPProtocolGenerators\AMQPProtocolGenerators.csproj" />
	/// </summary>
	public class Declare : IFrameMethod {
		public short SourceClassId => 40;
		public short SourceMethodId => 10;
		public bool IsAsyncResponse => false;
		public bool HasBody => false;

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

	/// <summary>
	/// Generated by AMQPProtocolGenerators application. Method represents "declare-ok" from AMQP protocol.
	/// <see href="..\AMQPProtocolGenerators\AMQPProtocolGenerators.csproj" />
	/// </summary>
	public class DeclareOk : IFrameMethod {
		public short SourceClassId => 40;
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

	/// <summary>
	/// Generated by AMQPProtocolGenerators application. Method represents "delete" from AMQP protocol.
	/// <see href="..\AMQPProtocolGenerators\AMQPProtocolGenerators.csproj" />
	/// </summary>
	public class Delete : IFrameMethod {
		public short SourceClassId => 40;
		public short SourceMethodId => 20;
		public bool IsAsyncResponse => false;
		public bool HasBody => false;

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

	/// <summary>
	/// Generated by AMQPProtocolGenerators application. Method represents "delete-ok" from AMQP protocol.
	/// <see href="..\AMQPProtocolGenerators\AMQPProtocolGenerators.csproj" />
	/// </summary>
	public class DeleteOk : IFrameMethod {
		public short SourceClassId => 40;
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

}
