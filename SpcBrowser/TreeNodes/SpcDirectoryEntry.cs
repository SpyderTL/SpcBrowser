using System.IO;
using System.Linq;
using System.Text;

namespace SpcBrowser.TreeNodes
{
	public class SpcDirectoryEntry : DataNode
	{
		public IReadable Source;
		public int Start;
		public int Loop;

		public SpcDirectoryEntry() : base(true)
		{
		}

		public override void Reload()
		{
			Nodes.Clear();

			using (var reader = new BinaryReader(Source.GetStream()))
			{
				reader.BaseStream.Position = 0x100 + Start;

				var block = 0;

				while (true)
				{
					var header = reader.ReadByte();

					var range = header >> 4;
					var filter = (header >> 2) & 0x3;
					var loop = (header & 0x2) != 0;
					var end = (header & 0x1) != 0;

					var data = reader.ReadBytes(8);
					var samples = data.SelectMany(x => new int[] { SignedNibbleToInt(x >> 4), SignedNibbleToInt(x & 0xf) }).ToArray();

					Nodes.Add(new SpcCompressedAudioBlock { Text = block.ToString(), Range = range, Filter = filter, Loop = loop, End = end, Samples = samples });

					block++;

					if (end)
						break;
				}
			}
		}

		private int SignedNibbleToInt(int value)
		{
			if (value < 8)
				return value;

			return value - 16;
		}

		public override object GetProperties()
		{
			return new
			{
				Start,
				Loop
			};
		}
	}
}