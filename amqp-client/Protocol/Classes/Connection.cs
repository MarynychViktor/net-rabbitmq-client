namespace AMQPClient.Protocol.Classes;

public static class Connection {
	public class Start : IFrameMethod {
		public short SourceClassId => 10;
		public short SourceMethodId => 10;
		public bool IsAsyncResponse => false;
		public bool HasBody => false;

		public byte VersionMajor { get; set; }
		public byte VersionMinor { get; set; }
		public Dictionary<string, object> ServerProperties { get; set; }
		public string Mechanisms { get; set; }
		public string Locales { get; set; }

		public byte[] Serialize() {
			var writer = new BinWriter();
			writer.WriteShort(SourceClassId);
			writer.WriteShort(SourceMethodId);
			writer.WriteByte(VersionMajor);
			writer.WriteByte(VersionMinor);
			writer.WriteFieldTable(ServerProperties);
			writer.WriteLongStr(Mechanisms);
			writer.WriteLongStr(Locales);
			return writer.ToArray();
		}

		public void Deserialize(byte[] bytes) {
			var reader = new BinReader(bytes);
			reader.ReadShort();
			reader.ReadShort();
			VersionMajor = reader.ReadByte();
			VersionMinor = reader.ReadByte();
			ServerProperties = reader.ReadFieldTable();
			Mechanisms = reader.ReadLongStr();
			Locales = reader.ReadLongStr();
		}
	}

	public class StartOk : IFrameMethod {
		public short SourceClassId => 10;
		public short SourceMethodId => 11;
		public bool IsAsyncResponse => true;
		public bool HasBody => false;

		public Dictionary<string, object> ClientProperties { get; set; }
		public string Mechanism { get; set; }
		public string Response { get; set; }
		public string Locale { get; set; }

		public byte[] Serialize() {
			var writer = new BinWriter();
			writer.WriteShort(SourceClassId);
			writer.WriteShort(SourceMethodId);
			writer.WriteFieldTable(ClientProperties);
			writer.WriteShortStr(Mechanism);
			writer.WriteLongStr(Response);
			writer.WriteShortStr(Locale);
			return writer.ToArray();
		}

		public void Deserialize(byte[] bytes) {
			var reader = new BinReader(bytes);
			reader.ReadShort();
			reader.ReadShort();
			ClientProperties = reader.ReadFieldTable();
			Mechanism = reader.ReadShortStr();
			Response = reader.ReadLongStr();
			Locale = reader.ReadShortStr();
		}
	}

	public class Secure : IFrameMethod {
		public short SourceClassId => 10;
		public short SourceMethodId => 20;
		public bool IsAsyncResponse => false;
		public bool HasBody => false;

		public string Challenge { get; set; }

		public byte[] Serialize() {
			var writer = new BinWriter();
			writer.WriteShort(SourceClassId);
			writer.WriteShort(SourceMethodId);
			writer.WriteLongStr(Challenge);
			return writer.ToArray();
		}

		public void Deserialize(byte[] bytes) {
			var reader = new BinReader(bytes);
			reader.ReadShort();
			reader.ReadShort();
			Challenge = reader.ReadLongStr();
		}
	}

	public class SecureOk : IFrameMethod {
		public short SourceClassId => 10;
		public short SourceMethodId => 21;
		public bool IsAsyncResponse => true;
		public bool HasBody => false;

		public string Response { get; set; }

		public byte[] Serialize() {
			var writer = new BinWriter();
			writer.WriteShort(SourceClassId);
			writer.WriteShort(SourceMethodId);
			writer.WriteLongStr(Response);
			return writer.ToArray();
		}

		public void Deserialize(byte[] bytes) {
			var reader = new BinReader(bytes);
			reader.ReadShort();
			reader.ReadShort();
			Response = reader.ReadLongStr();
		}
	}

	public class Tune : IFrameMethod {
		public short SourceClassId => 10;
		public short SourceMethodId => 30;
		public bool IsAsyncResponse => false;
		public bool HasBody => false;

		public short ChannelMax { get; set; }
		public int FrameMax { get; set; }
		public short Heartbeat { get; set; }

		public byte[] Serialize() {
			var writer = new BinWriter();
			writer.WriteShort(SourceClassId);
			writer.WriteShort(SourceMethodId);
			writer.WriteShort(ChannelMax);
			writer.WriteInt(FrameMax);
			writer.WriteShort(Heartbeat);
			return writer.ToArray();
		}

		public void Deserialize(byte[] bytes) {
			var reader = new BinReader(bytes);
			reader.ReadShort();
			reader.ReadShort();
			ChannelMax = reader.ReadShort();
			FrameMax = reader.ReadInt();
			Heartbeat = reader.ReadShort();
		}
	}

	public class TuneOk : IFrameMethod {
		public short SourceClassId => 10;
		public short SourceMethodId => 31;
		public bool IsAsyncResponse => true;
		public bool HasBody => false;

		public short ChannelMax { get; set; }
		public int FrameMax { get; set; }
		public short Heartbeat { get; set; }

		public byte[] Serialize() {
			var writer = new BinWriter();
			writer.WriteShort(SourceClassId);
			writer.WriteShort(SourceMethodId);
			writer.WriteShort(ChannelMax);
			writer.WriteInt(FrameMax);
			writer.WriteShort(Heartbeat);
			return writer.ToArray();
		}

		public void Deserialize(byte[] bytes) {
			var reader = new BinReader(bytes);
			reader.ReadShort();
			reader.ReadShort();
			ChannelMax = reader.ReadShort();
			FrameMax = reader.ReadInt();
			Heartbeat = reader.ReadShort();
		}
	}

	public class Open : IFrameMethod {
		public short SourceClassId => 10;
		public short SourceMethodId => 40;
		public bool IsAsyncResponse => false;
		public bool HasBody => false;

		public string VirtualHost { get; set; }
		public string Reserved1 { get; set; }= "";
		public bool Reserved2 { get; set; }

		public byte[] Serialize() {
			var writer = new BinWriter();
			writer.WriteShort(SourceClassId);
			writer.WriteShort(SourceMethodId);
			writer.WriteShortStr(VirtualHost);
			writer.WriteShortStr(Reserved1);
			writer.WriteBit((byte)(Reserved2 ? 1 : 0), false);
			return writer.ToArray();
		}

		public void Deserialize(byte[] bytes) {
			var reader = new BinReader(bytes);
			reader.ReadShort();
			reader.ReadShort();
			VirtualHost = reader.ReadShortStr();
			Reserved1 = reader.ReadShortStr();
			var flags = reader.ReadByte();
			Reserved2 = (flags & 1) > 0;
		}
	}

	public class OpenOk : IFrameMethod {
		public short SourceClassId => 10;
		public short SourceMethodId => 41;
		public bool IsAsyncResponse => true;
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

	public class Close : IFrameMethod {
		public short SourceClassId => 10;
		public short SourceMethodId => 50;
		public bool IsAsyncResponse => false;
		public bool HasBody => false;

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
		public short SourceClassId => 10;
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

}
