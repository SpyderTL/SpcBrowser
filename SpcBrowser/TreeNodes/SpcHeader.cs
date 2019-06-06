using System.IO;
using System.Text;

namespace SpcBrowser.TreeNodes
{
	public class SpcHeader : DataNode
	{
		public IReadable Source;

		public SpcHeader() : base(false)
		{
		}

		public override void Reload()
		{
		}

		public override object GetProperties()
		{
			using (var reader = new BinaryReader(Source.GetStream()))
			{
				var signature = Encoding.ASCII.GetString(reader.ReadBytes(33));
				var reserved = reader.ReadBytes(2);
				var tag = reader.ReadByte();
				var version = reader.ReadByte();

				var PC = reader.ReadUInt16();
				var A = reader.ReadByte();
				var X = reader.ReadByte();
				var Y = reader.ReadByte();
				var PSW = reader.ReadByte();
				var SP = reader.ReadByte();
				var reserved2 = reader.ReadBytes(2);

				return new
				{
					Signature = signature,
					Reserved = reserved,
					Tag = tag,
					Version = version,
					PC,
					A,
					X,
					Y,
					PSW,
					SP,
					Reserved2 = reserved2
				};
			}
		}
	}
}