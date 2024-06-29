using AMQPClient.Protocol;

public class Connection {
	public class Start {
		public const short SourceClassId = 10;
		public const short SourceMethodId = 10;
		public const bool IsAsyncResponse = false;
		public const bool HasBody = false;

		public bool VersionMajor {get;set;}
		public bool VersionMinor {get;set;}
		public Dictionary<string, object> ServerProperties {get;set;}
		public string Mechanisms {get;set;}
		public string Locales {get;set;}

		public byte[] Serialize() {
			var writer = new BinWriter();
			writer.WriteShort(SourceClassId);
			writer.WriteShort(SourceMethodId);
			writer.WriteBit((byte)(VersionMajor ? 1 : 0), false);
			writer.WriteBit((byte)(VersionMinor ? 2 : 0), true);
			writer.WriteFieldTable(ServerProperties);
			writer.WriteLongStr(Mechanisms);
			writer.WriteLongStr(Locales);
			return writer.ToArray();
		}

		public void Deserialize(byte[] bytes) {
			var reader = new BinReader(bytes);
			reader.ReadShort();
			reader.ReadShort();
			var flags = reader.ReadByte();
			VersionMajor = (flags & 1) > 0;
			VersionMinor = (flags & 2) > 0;
			ServerProperties = reader.ReadFieldTable();
			Mechanisms = reader.ReadLongStr();
			Locales = reader.ReadLongStr();
		}
	}

	public class StartOk {
		public const short SourceClassId = 10;
		public const short SourceMethodId = 11;
		public const bool IsAsyncResponse = true;
		public const bool HasBody = false;

		public Dictionary<string, object> ClientProperties {get;set;}
		public string Mechanism {get;set;}
		public string Response {get;set;}
		public string Locale {get;set;}

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

	public class Secure {
		public const short SourceClassId = 10;
		public const short SourceMethodId = 20;
		public const bool IsAsyncResponse = false;
		public const bool HasBody = false;

		public string Challenge {get;set;}

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

	public class SecureOk {
		public const short SourceClassId = 10;
		public const short SourceMethodId = 21;
		public const bool IsAsyncResponse = true;
		public const bool HasBody = false;

		public string Response {get;set;}

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

	public class Tune {
		public const short SourceClassId = 10;
		public const short SourceMethodId = 30;
		public const bool IsAsyncResponse = false;
		public const bool HasBody = false;

		public short ChannelMax {get;set;}
		public int FrameMax {get;set;}
		public short Heartbeat {get;set;}

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

	public class TuneOk {
		public const short SourceClassId = 10;
		public const short SourceMethodId = 31;
		public const bool IsAsyncResponse = true;
		public const bool HasBody = false;

		public short ChannelMax {get;set;}
		public int FrameMax {get;set;}
		public short Heartbeat {get;set;}

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

	public class Open {
		public const short SourceClassId = 10;
		public const short SourceMethodId = 40;
		public const bool IsAsyncResponse = false;
		public const bool HasBody = false;

		public string VirtualHost {get;set;}
		public string Reserved1 {get;set;}
		public bool Reserved2 {get;set;}

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

	public class OpenOk {
		public const short SourceClassId = 10;
		public const short SourceMethodId = 41;
		public const bool IsAsyncResponse = true;
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

	public class Close {
		public const short SourceClassId = 10;
		public const short SourceMethodId = 50;
		public const bool IsAsyncResponse = false;
		public const bool HasBody = false;

		public short ReplyCode {get;set;}
		public string ReplyText {get;set;}
		public short ClassId {get;set;}
		public short MethodId {get;set;}

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
		public const short SourceClassId = 10;
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

}
