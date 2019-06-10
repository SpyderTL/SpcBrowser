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
		private bool Loop;

		public BrowserForm()
		{
			InitializeComponent();

			var examples = new Folder { Text = "Examples", Path = @"..\..\Examples" };

			treeView.Nodes.Add(examples);

			Audio = new XAudio2();
			Audio.StartEngine();

			Master = new MasteringVoice(Audio);

			var format = new WaveFormat(32000, 16, 1);

			Voice = new SourceVoice(Audio, format, VoiceFlags.None, 4.0f);

			Voice.BufferEnd += Voice_BufferEnd;

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
			Loop = false;
			Play();
		}

		private void loopButton_Click(object sender, EventArgs e)
		{
			Loop = true;
			Play();
		}

		private void trackBar_Scroll(object sender, EventArgs e)
		{
			speedTextBox.Text = (trackBar.Value * 0.04f).ToString();
		}

		private void playMenuItem_Click(object sender, EventArgs e)
		{
			Loop = false;
			Play();
		}

		private void Play()
		{
			if (!(treeView.SelectedNode is SpcDirectoryEntry entry))
				return;

			if (entry.Start > entry.Loop ||
				entry.Start == 0 ||
				entry.Loop == 0)
				return;

			Voice.SetFrequencyRatio(float.Parse(speedTextBox.Text));

			var stream = entry.Source.GetStream();
			var reader = new BinaryReader(stream);

			stream.Position = 0x100 + entry.Start;

			//var blocks = (entry.Loop - entry.Start) / 9;

			//var data2 = new short[blocks * 16];
			var data2 = new List<short>();
			//var index = 0;

			while (true)
			{
				var header = reader.ReadByte();

				var range = header >> 4;
				var filter = (header >> 2) & 0x3;
				var loop = (header & 0x2) != 0;
				var end = (header & 0x1) != 0;

				var data = reader.ReadBytes(8);
				var samples = data.SelectMany(x => new int[] { SignedNibbleToInt(x >> 4), SignedNibbleToInt(x & 0xf) }).ToArray();
				//var samples = data.SelectMany(x => new int[] { SignedNibbleToInt(x & 0xf), SignedNibbleToInt(x >> 4) }).ToArray();

				foreach (var sample in samples)
				{
					var value = sample << range;

					switch (filter)
					{
						case 1:
							if(data2.Count > 0)
								value += (int)(data2[data2.Count - 1] * (15.0f / 16.0f));
							break;

						case 2:
							if(data2.Count > 1)
								value += (int)((data2[data2.Count - 1] * (61.0f / 32.0f)) - (data2[data2.Count - 2] * (15.0f / 16.0f)));
							break;

						case 3:
							if(data2.Count > 1)
								value += (int)((data2[data2.Count - 1] * (115.0f / 64.0f)) - (data2[data2.Count - 2] * (13.0f / 16.0f)));
							break;
					}

					data2.Add((short)value);
				}

				if (end)
					break;
			}

			var data3 = data2.ToArray();

			Buffers = new AudioBuffer[2];
			Pointers = new DataPointer[Buffers.Length];
			//Data = new byte[1024];
			//Random = new Random();

			for (int buffer = 0; buffer < Buffers.Length; buffer++)
			{
				Pointers[buffer] = new DataPointer(Utilities.AllocateClearedMemory(data3.Length * 2), data3.Length * 2);
				Buffers[buffer] = new AudioBuffer(Pointers[buffer]);

				Pointers[buffer].CopyFrom(data3);

				if (Loop || buffer == 0)
					Voice.SubmitSourceBuffer(Buffers[buffer], null);
			}

			Index = 0;

			//Pointers[0] = new DataPointer(Utilities.AllocateClearedMemory(data2.Length * 2), data2.Length * 2);
			//Buffers[0] = new AudioBuffer(Pointers[0]);

			//Pointers[0].CopyFrom(data2);

			//Voice.SubmitSourceBuffer(Buffers[0], null);
		}

		private void Voice_BufferEnd(IntPtr obj)
		{
			if (Loop)
			{
				//for (var x = 0; x < Data.Length; x++)
				//	Data[x] = (byte)Random.Next(0, 25);

				//Pointers[Index].CopyFrom(Data);

				Voice.SubmitSourceBuffer(Buffers[Index], null);

				Index++;

				if (Index == Buffers.Length)
					Index = 0;
			}
		}

		private int SignedNibbleToInt(int value)
		{
			if (value < 8)
				return value;

			return value - 16;
		}
	}
}
