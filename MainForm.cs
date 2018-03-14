/*
 * Created by SharpDevelop.
 * User: NCDyson
 * Date: 1/5/2018
 * Time: 4:09 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Diagnostics;


namespace RareView
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : Form
	{
		GLControl glWin = null;
		public Timer RenderTimer = new Timer();
		
		public MainForm()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			Logger.SetLogControl(richTextBox1);
			glWin = new GLControl();
			splitContainer2.Panel2.Controls.Add(glWin);
			OpenTK.Toolkit.Init();
			glWin.CreateControl();
			glWin.CreateGraphics();
			glWin.Dock = DockStyle.Fill;
			glWin.BackColor = Color.DarkGray;
			glWin.BringToFront();
			
			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
			Scene.Init(glWin);
			glWin.KeyDown += glWin_KeyPress;
			glWin.KeyUp += glWin_KeyRelease;
			glWin.MouseMove += glWin_MouseMove;
			glWin.MouseWheel += glWin_MouseWheel;
			glWin.Paint += glWin_Paint;
			RenderTimer.Interval = 15;
			RenderTimer.Tick += Repaint;
			RenderTimer.Start();
		}
		
		public void glWin_Paint(object sender, PaintEventArgs e)
		{
			Scene.Render();
		}
		
		public void glWin_KeyPress(object sender, KeyEventArgs e)
		{
			Scene.KeyPress(e);
		}
		
		public void glWin_KeyRelease(object sender, KeyEventArgs e)
		{
			Scene.KeyRelease(e);
		}
		
		public void glWin_MouseMove(object sender, MouseEventArgs e)
		{
			Scene.MouseMove(e);
		}
		
		public void glWin_MouseWheel(object sender, MouseEventArgs e)
		{
			Scene.MouseWheel(e);
		}
		
		public void Repaint(object sender, EventArgs e)
		{
			Scene.Render();
		}
		
		void LoadToolStripMenuItemClick(object sender, EventArgs e)
		{
			var dlg = new OpenFileDialog();
			dlg.Filter = "Kameo Caff Files (*.mdl, *.lvl)|*.mdl;*.lvl|All Files (*.*)|*.*";
			dlg.Title = "Select files to load...";
			dlg.Multiselect = true;
			
			if(dlg.ShowDialog() != DialogResult.OK) return;
			LoadFiles(dlg.FileNames);
		}
		
		void LoadFiles(string[] fileNames)
		{
			foreach(var tmpFileName in fileNames)
			{
				Debug.WriteLine(string.Format("Loading {0}...", tmpFileName));
				var retNode = Scene.LoadCaffFile(tmpFileName);
				if(retNode != null)
				{
					
					treeView1.Nodes.Add(retNode);
				}
			}
		}
		void TreeView1NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			if(e.Button == MouseButtons.Right)
			{
				treeView1.SelectedNode = e.Node;
				var node = e.Node;
				if(node.Tag != null)
				{
					var tag = (TreeNodeTag)node.Tag;
					if(tag.ObjectType == TreeNodeTag.OBJECT_TYPE_RENDERGRAPH_VERTEX_BATCH)
					{
						vertexBatchContextStrip.Show(treeView1, e.X, e.Y);
					}
					else if(tag.ObjectType == TreeNodeTag.OBJECT_TYPE_TEXTURE)
					{
						textureContextStrip.Show(treeView1, e.X, e.Y);
					}
					else if(tag.Type == TreeNodeTag.NodeType.File)
					{
						CaffFileContextMenu.Show(treeView1, e.X, e.Y);
					}
				}
			}
		}
		void TreeView1AfterSelect(object sender, TreeViewEventArgs e)
		{
			var node = e.Node;
			Scene.SelectedItemTag = (TreeNodeTag)node.Tag;
			propertyGrid1.SelectedObject = Scene.GetSelectedObject();

		}
		void MainFormFormClosing(object sender, FormClosingEventArgs e)
		{
			Scene.DeInit();
		}
		void SetDefaultTexCoordScaleToolStripMenuItemClick(object sender, EventArgs e)
		{
			var node = treeView1.SelectedNode;
			if(node != null)
			{
				var nodeTag = (TreeNodeTag)node.Tag;
				if(nodeTag != null)
				{
					var file = nodeTag.File;
					var graph = file.GetRenderGraph(nodeTag.ObjectID);
					if(graph != null)
					{
						var vBatch = graph.GetVertexBatch(nodeTag.SubObjectID);
						if(vBatch != null)
						{
							graph.SetDefaultUVScale(vBatch.VD_TexCoordScale);
						}
					}
				}
			}
		}
		void ExportToolStripMenuItemClick(object sender, EventArgs e)
		{
			var node = treeView1.SelectedNode;
			var nodeTag = (TreeNodeTag)node.Tag;
			if(nodeTag != null)
			{
				var nodeFile = nodeTag.File;
				var vfd = new SaveFileDialog();
				vfd.CheckFileExists = false;
				vfd.FileName = "Export Here";
				vfd.Title = "Select directory to export to...";
				DialogResult res = vfd.ShowDialog();
				if(res == DialogResult.OK)
				{
					string savePath = System.IO.Path.GetDirectoryName(vfd.FileName);
					nodeFile.DumpToOBJ(savePath);
				}
			}
		}
		void WireframeOverlayToolStripMenuItemCheckedChanged(object sender, EventArgs e)
		{
			Scene.RenderWireframeOverlay = wireframeOverlayToolStripMenuItem.Checked;
		}
		
		void BackfaceCullingToolStripMenuItemCheckedChanged(object sender, EventArgs e)
		{
			Scene.BackfaceCulling = backfaceCullingToolStripMenuItem.Checked;
		}
		void RecropToolStripMenuItemClick(object sender, EventArgs e)
		{
			var node = treeView1.SelectedNode;
			var nodeTag = (TreeNodeTag)node.Tag;
			if(nodeTag != null)
			{
				var tmpFile = nodeTag.File;
				tmpFile.RecropTexture(nodeTag.ObjectID);
			}
		}
		
		void DumpToXprToolStripMenuItemClick(object sender, EventArgs e)
		{
			var node = treeView1.SelectedNode;
			var nodeTag = (TreeNodeTag)node.Tag;
			if(nodeTag != null)
			{
				var tmpFile = nodeTag.File;
				tmpFile.DumpTextureToXpr(nodeTag.ObjectID);
			}
		}
		
		void Tbtn3DViewCheckedChanged(object sender, EventArgs e)
		{
			if(tbtn3DView.Checked)
			{
				tbtnUVView.Checked = false;
				Scene.ViewMode = Scene.VIEW_MODE_3D;
			}
			else
			{
				tbtnUVView.Checked = true;
			}
		}
		void TbtnUVViewCheckedChanged(object sender, EventArgs e)
		{
			if(tbtnUVView.Checked)
			{
				tbtn3DView.Checked = false;
				Scene.ViewMode = Scene.VIEW_MODE_UV;
			}
			else
			{
				tbtn3DView.Checked = true;
			}
		}
		void LogWindowToolStripMenuItemCheckedChanged(object sender, EventArgs e)
		{
			splitContainer1.Panel2Collapsed = !logWindowToolStripMenuItem.Checked;
		}
		void UnloadToolStripMenuItemClick(object sender, EventArgs e)
		{
			var selNode = treeView1.SelectedNode;
			treeView1.SelectedNode = null;
			treeView1.Nodes.Remove(selNode);
			
			var tag = (TreeNodeTag)selNode.Tag;
			if(tag != null)
			{
				if(tag.File != null)
				{
					Scene.UnloadCaffFile(tag.File);
				}
			}
		}
		void VertexPointOverlayToolStripMenuItemCheckedChanged(object sender, EventArgs e)
		{
			Scene.RenderVertexPointOverlay = vertexPointOverlayToolStripMenuItem.Checked;
		}


	}
}
