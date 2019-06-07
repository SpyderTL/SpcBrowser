using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Multimedia;
using SharpDX.XAudio2;
using SpcBrowser.TreeNodes;

namespace SpcBrowser
{
	public partial class BrowserForm : Form
	{
		private XAudio2 Audio;
		private MasteringVoice Master;
		private SourceVoice Voice;
		private AudioBuffer[] Buffers;
		private DataPointer[] Pointers;
		private byte[] Data;
		private int Index;
		private Random Random;

		public BrowserForm()
		{
			InitializeComponent();

			treeView.Nodes.Add(new SpcFile
			{
				Text = "104 Title Demonstration.spc",
				Path = @"..\..\Examples\104 Title Demonstration.spc"
			});

			treeView.Nodes.Add(new SpcFile
			{
				Text = "105 Title Screen.spc",
				Path = @"..\..\Examples\105 Title Screen.spc"
			});

			treeView.Nodes.Add(new SpcFile
			{
				Text = "133 Last Boss Clear.spc",
				Path = @"..\..\Examples\133 Last Boss Clear.spc"
			});

			treeView.Nodes.Add(new SpcFile
			{
				Text = "994 Briefing.spc",
				Path = @"..\..\Examples\994 Briefing.spc"
			});

			Audio = new XAudio2();
			Audio.StartEngine();

			Master = new MasteringVoice(Audio);

			var format = new WaveFormat(32000, 16, 1);

			Voice = new SourceVoice(Audio, format);

			//Voice.BufferEnd += Voice_BufferEnd;

			Voice.Start();
		}

		private void treeView_AfterExpand(object sender, TreeViewEventArgs e)
		{
			if (e.Node is DataNode)
				((DataNode)e.Node).Reload();
		}

		private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
		{
			if (e.Node is DataNode)
				propertyGrid.SelectedObject = ((DataNode)e.Node).GetProperties();
			else
				propertyGrid.SelectedObject = null;
		}

		private void playButton_Click(object sender, EventArgs e)
		{
			Play();
		}

		private void Play()
		{
			if (!(treeView.SelectedNode is SpcDirectoryEntry entry))
				return;

			var stream = entry.Source.GetStream();
			var reader = new BinaryReader(stream);

			stream.Position = 0x100 + entry.Start;

			var blocks = (entry.Loop - entry.Start) / 9;

			var data2 = new short[blocks * 16];
			var index = 0;

			for(var block = 0; block < blocks; block++)
			{
				var header = reader.ReadByte();

				var range = header >> 4;
				var filter = (header >> 2) & 0x3;
				var loop = (header & 0x2) != 0;
				var end = (header & 0x1) != 0;

				var data = reader.ReadBytes(8);
				var samples = data.SelectMany(x => new int[] { NibbleToInt(x >> 4), NibbleToInt(x & 0xf) }).ToArray();

				foreach (var sample in samples)
					data2[index++] = (short)(sample << range);

				if (end)
					break;
			}

			Buffers = new AudioBuffer[2];
			Pointers = new DataPointer[Buffers.Length];
			//Data = new byte[1024];
			//Random = new Random();

			//for (int buffer = 0; buffer < Buffers.Length; buffer++)
			//{
			//	Pointers[buffer] = new DataPointer(Utilities.AllocateClearedMemory(1024), 1024);
			//	Buffers[buffer] = new AudioBuffer(Pointers[buffer]);

			//	Voice.SubmitSourceBuffer(Buffers[buffer], null);
			//}

			//Index = 0;

			Pointers[0] = new DataPointer(Utilities.AllocateClearedMemory(data2.Length * 2), data2.Length * 2);
			Buffers[0] = new AudioBuffer(Pointers[0]);

			Pointers[0].CopyFrom(data2);

			Voice.SubmitSourceBuffer(Buffers[0], null);
		}

		private void Voice_BufferEnd(IntPtr obj)
		{
			for (var x = 0; x < Data.Length; x++)
				Data[x] = (byte)Random.Next(0, 25);

			Pointers[Index].CopyFrom(Data);

			Voice.SubmitSourceBuffer(Buffers[Index], null);

			Index++;

			if (Index == Buffers.Length)
				Index = 0;
		}

		private int NibbleToInt(int value)
		{
			if (value < 8)
				return value;

			return value - 16;
		}
	}
}
