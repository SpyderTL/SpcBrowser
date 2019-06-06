using System.IO;
using System.Text;

namespace SpcBrowser.TreeNodes
{
	public class SpcFooter : DataNode
	{
		public IReadable Source;

		public SpcFooter() : base(false)
		{
		}

		public override void Reload()
		{
		}

		public override object GetProperties()
		{
			using (var reader = new BinaryReader(Source.GetStream()))
			{
				reader.BaseStream.Position = 0x10100;

				var dspRegisters = reader.ReadBytes(128);

				return new
				{
					DspRegisters = dspRegisters
				};
			}
		}
	}
}