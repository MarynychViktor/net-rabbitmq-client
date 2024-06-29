using AMQPClient.Protocol;

public class Tx {
	public class Select {
		public const short SourceClassId = 90;
		public const short SourceMethodId = 10;
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

	public class SelectOk {
		public const short SourceClassId = 90;
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

	public class Commit {
		public const short SourceClassId = 90;
		public const short SourceMethodId = 20;
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

	public class CommitOk {
		public const short SourceClassId = 90;
		public const short SourceMethodId = 21;
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

	public class Rollback {
		public const short SourceClassId = 90;
		public const short SourceMethodId = 30;
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

	public class RollbackOk {
		public const short SourceClassId = 90;
		public const short SourceMethodId = 31;
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
