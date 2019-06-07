using System.IO;
using System.Linq;
using System.Text;

namespace SpcBrowser.TreeNodes
{
	public class SpcCompressedAudioBlock : DataNode
	{
		//public IReadable Source;
		//public int Offset;
		public int Range;
		public int Filter;
		public bool Loop;
		public bool End;
		public int[] Samples;

		public SpcCompressedAudioBlock() : base(false)
		{
		}

		public override void Reload()
		{
		}

		public override object GetProperties()
		{
			//using (var reader = new BinaryReader(Source.GetStream()))
			//{
			//	reader.BaseStream.Position = 0x100 + Offset;

			//	var header = reader.ReadByte();

			//	var range = header >> 4;
			//	var filter = (header >> 2) & 0x3;
			//	var loop = (header & 0x2) != 0;
			//	var end = (header & 0x1) != 0;

			//	var data = reader.ReadBytes(8);
			//	var samples = data.SelectMany(x => new int[] { x >> 4, x & 0xf }).ToArray();

				return new
				{
					Range,
					Filter,
					Loop,
					End,
					Samples
				};
			//}
		}
	}
}