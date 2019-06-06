using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SpcBrowser.TreeNodes;

namespace SpcBrowser
{
	public partial class BrowserForm : Form
	{
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
	}
}
