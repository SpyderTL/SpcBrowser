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

			for (var block = 0; block < 256; block++)
				Nodes.Add(new SpcCompressedAudioBlock { Text = block.ToString(), Offset = Start + (block * 9), Source = Source });
		}

		public override object GetProperties()
		{
			return new
			{
				Start = Start,
				Loop = Loop
			};
		}
	}
}