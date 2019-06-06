using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpcBrowser.TreeNodes
{
	public class SpcFile : DataNode, IReadable
	{
		public string Path;

		public SpcFile() : base(true)
		{
		}

		public override void Reload()
		{
			Nodes.Clear();

			Nodes.Add(new SpcHeader { Text = "SPC Header", Source = this });
		}

		public override object GetProperties()
		{
			return new { Path };
		}

		public Stream GetStream()
		{
			return File.OpenRead(Path);
		}
	}
}
