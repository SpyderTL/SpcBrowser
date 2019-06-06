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
		private AudioBuffer[] Buffers;
		private DataPointer[] Pointers;
		private byte[] Data;
		private int Index;
		private SourceVoice Voice;

		public BrowserForm()
		{
			InitializeComponent();

			treeView.Nodes.Add(new SpcFile
			{
				Text = "104 Title Demonstration.spc",
				Path = @"..\..\Examples\104 Title Demonstration.spc"
			});
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
			var audio = new XAudio2();
			audio.StartEngine();

			var master = new MasteringVoice(audio);

			var format = new WaveFormat(32000, 16, 1);

			Voice = new SourceVoice(audio, format);

			Voice.BufferEnd += Voice_BufferEnd;

			Voice.Start();

			Buffers = new AudioBuffer[2];
			Pointers = new DataPointer[Buffers.Length];
			Data = new byte[1024];

			for (int buffer = 0; buffer < Buffers.Length; buffer++)
			{
				Pointers[buffer] = new DataPointer(Utilities.AllocateClearedMemory(1024), 1024);
				Buffers[buffer] = new AudioBuffer(Pointers[buffer]);

				Voice.SubmitSourceBuffer(Buffers[buffer], null);
			}

			Index = 0;
		}

		private void Voice_BufferEnd(IntPtr obj)
		{
			var random = new Random();

			for (var x = 0; x < Data.Length; x++)
				Data[x] = (byte)random.Next(0, 25);

			Pointers[Index].CopyFrom(Data);

			Voice.SubmitSourceBuffer(Buffers[Index], null);

			Index++;

			if (Index == Buffers.Length)
				Index = 0;
		}
	}
}
