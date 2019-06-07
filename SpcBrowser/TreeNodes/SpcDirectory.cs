using System.IO;
using System.Linq;
using System.Text;

namespace SpcBrowser.TreeNodes
{
	public class SpcDirectory : DataNode
	{
		public IReadable Source;

		public SpcDirectory() : base(true)
		{
		}

		public override void Reload()
		{
			Nodes.Clear();

			using (var reader = new BinaryReader(Source.GetStream()))
			{
				reader.BaseStream.Position = 0x1015d;

				var sourceDirectory = reader.ReadByte() * 0x100;

				reader.BaseStream.Position = 0x100 + sourceDirectory;

				for (var entry = 0; entry < 256; entry++)
				{
					var start = reader.ReadUInt16();
					var loop = reader.ReadUInt16();

					Nodes.Add(new SpcDirectoryEntry { Text = entry.ToString("X2") + ": " + ((loop - start) <= 0 ? "Invalid" : (loop - start).ToString()), Start = start, Loop = loop, Source = Source });
				}
			}
		}

		public override object GetProperties()
		{
			return null;
		}
	}
}