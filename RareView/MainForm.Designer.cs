/*
 * Created by SharpDevelop.
 * User: NCDyson
 * Date: 1/5/2018
 * Time: 4:09 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace RareView
{
	partial class MainForm
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.SplitContainer splitContainer2;
		private System.Windows.Forms.TreeView treeView1;
		private System.Windows.Forms.PropertyGrid propertyGrid1;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.SplitContainer splitContainer3;
		private System.Windows.Forms.RichTextBox richTextBox1;
		private System.Windows.Forms.ContextMenuStrip CaffFileContextMenu;
		private System.Windows.Forms.ToolStripMenuItem unloadToolStripMenuItem;
		private System.Windows.Forms.ContextMenuStrip vertexBatchContextStrip;
		private System.Windows.Forms.ToolStripMenuItem setDefaultTexCoordScaleToolStripMenuItem;
		private System.Windows.Forms.ContextMenuStrip textureContextStrip;
		private System.Windows.Forms.ToolStripMenuItem recropToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
		private System.Windows.Forms.ToolStrip toolStrip1;
		private System.Windows.Forms.ToolStripSplitButton toolStripSplitButton1;
		private System.Windows.Forms.ToolStripMenuItem wireframeOverlayToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem backfaceCullingToolStripMenuItem;
		private System.Windows.Forms.ToolStripButton tbtn3DView;
		private System.Windows.Forms.ToolStripButton tbtnUVView;
		private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem logWindowToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem dumpToXprToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem vertexPointOverlayToolStripMenuItem;
		
		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.splitContainer2 = new System.Windows.Forms.SplitContainer();
			this.splitContainer3 = new System.Windows.Forms.SplitContainer();
			this.treeView1 = new System.Windows.Forms.TreeView();
			this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this.toolStripSplitButton1 = new System.Windows.Forms.ToolStripSplitButton();
			this.vertexPointOverlayToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.wireframeOverlayToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.backfaceCullingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.tbtn3DView = new System.Windows.Forms.ToolStripButton();
			this.tbtnUVView = new System.Windows.Forms.ToolStripButton();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.logWindowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.richTextBox1 = new System.Windows.Forms.RichTextBox();
			this.CaffFileContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.unloadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.vertexBatchContextStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.setDefaultTexCoordScaleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.textureContextStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.recropToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.dumpToXprToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
			this.splitContainer2.Panel1.SuspendLayout();
			this.splitContainer2.Panel2.SuspendLayout();
			this.splitContainer2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
			this.splitContainer3.Panel1.SuspendLayout();
			this.splitContainer3.Panel2.SuspendLayout();
			this.splitContainer3.SuspendLayout();
			this.toolStrip1.SuspendLayout();
			this.menuStrip1.SuspendLayout();
			this.CaffFileContextMenu.SuspendLayout();
			this.vertexBatchContextStrip.SuspendLayout();
			this.textureContextStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new System.Drawing.Point(0, 0);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
			this.splitContainer1.Panel1.Controls.Add(this.menuStrip1);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.BackColor = System.Drawing.SystemColors.ActiveBorder;
			this.splitContainer1.Panel2.Controls.Add(this.richTextBox1);
			this.splitContainer1.Panel2Collapsed = true;
			this.splitContainer1.Size = new System.Drawing.Size(883, 533);
			this.splitContainer1.SplitterDistance = 388;
			this.splitContainer1.TabIndex = 0;
			// 
			// splitContainer2
			// 
			this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer2.Location = new System.Drawing.Point(0, 24);
			this.splitContainer2.Name = "splitContainer2";
			// 
			// splitContainer2.Panel1
			// 
			this.splitContainer2.Panel1.Controls.Add(this.splitContainer3);
			// 
			// splitContainer2.Panel2
			// 
			this.splitContainer2.Panel2.Controls.Add(this.toolStrip1);
			this.splitContainer2.Size = new System.Drawing.Size(883, 509);
			this.splitContainer2.SplitterDistance = 294;
			this.splitContainer2.TabIndex = 2;
			// 
			// splitContainer3
			// 
			this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer3.Location = new System.Drawing.Point(0, 0);
			this.splitContainer3.Name = "splitContainer3";
			this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer3.Panel1
			// 
			this.splitContainer3.Panel1.Controls.Add(this.treeView1);
			// 
			// splitContainer3.Panel2
			// 
			this.splitContainer3.Panel2.Controls.Add(this.propertyGrid1);
			this.splitContainer3.Size = new System.Drawing.Size(294, 509);
			this.splitContainer3.SplitterDistance = 245;
			this.splitContainer3.TabIndex = 0;
			// 
			// treeView1
			// 
			this.treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeView1.Location = new System.Drawing.Point(0, 0);
			this.treeView1.Name = "treeView1";
			this.treeView1.Size = new System.Drawing.Size(294, 245);
			this.treeView1.TabIndex = 0;
			this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeView1AfterSelect);
			this.treeView1.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.TreeView1NodeMouseClick);
			// 
			// propertyGrid1
			// 
			this.propertyGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.propertyGrid1.LineColor = System.Drawing.SystemColors.ControlDark;
			this.propertyGrid1.Location = new System.Drawing.Point(0, 0);
			this.propertyGrid1.Name = "propertyGrid1";
			this.propertyGrid1.Size = new System.Drawing.Size(294, 260);
			this.propertyGrid1.TabIndex = 0;
			// 
			// toolStrip1
			// 
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.toolStripSplitButton1,
			this.tbtn3DView,
			this.tbtnUVView});
			this.toolStrip1.Location = new System.Drawing.Point(0, 0);
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.Size = new System.Drawing.Size(585, 25);
			this.toolStrip1.TabIndex = 0;
			this.toolStrip1.Text = "toolStrip1";
			// 
			// toolStripSplitButton1
			// 
			this.toolStripSplitButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.toolStripSplitButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.vertexPointOverlayToolStripMenuItem,
			this.wireframeOverlayToolStripMenuItem,
			this.backfaceCullingToolStripMenuItem});
			this.toolStripSplitButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripSplitButton1.Image")));
			this.toolStripSplitButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripSplitButton1.Name = "toolStripSplitButton1";
			this.toolStripSplitButton1.Size = new System.Drawing.Size(65, 22);
			this.toolStripSplitButton1.Text = "Options";
			// 
			// vertexPointOverlayToolStripMenuItem
			// 
			this.vertexPointOverlayToolStripMenuItem.CheckOnClick = true;
			this.vertexPointOverlayToolStripMenuItem.Name = "vertexPointOverlayToolStripMenuItem";
			this.vertexPointOverlayToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
			this.vertexPointOverlayToolStripMenuItem.Text = "Vertex Point Overlay";
			this.vertexPointOverlayToolStripMenuItem.CheckedChanged += new System.EventHandler(this.VertexPointOverlayToolStripMenuItemCheckedChanged);
			// 
			// wireframeOverlayToolStripMenuItem
			// 
			this.wireframeOverlayToolStripMenuItem.CheckOnClick = true;
			this.wireframeOverlayToolStripMenuItem.Name = "wireframeOverlayToolStripMenuItem";
			this.wireframeOverlayToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
			this.wireframeOverlayToolStripMenuItem.Text = "Wireframe Overlay";
			this.wireframeOverlayToolStripMenuItem.CheckedChanged += new System.EventHandler(this.WireframeOverlayToolStripMenuItemCheckedChanged);
			// 
			// backfaceCullingToolStripMenuItem
			// 
			this.backfaceCullingToolStripMenuItem.CheckOnClick = true;
			this.backfaceCullingToolStripMenuItem.Name = "backfaceCullingToolStripMenuItem";
			this.backfaceCullingToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
			this.backfaceCullingToolStripMenuItem.Text = "Backface Culling";
			this.backfaceCullingToolStripMenuItem.CheckedChanged += new System.EventHandler(this.BackfaceCullingToolStripMenuItemCheckedChanged);
			// 
			// tbtn3DView
			// 
			this.tbtn3DView.Checked = true;
			this.tbtn3DView.CheckOnClick = true;
			this.tbtn3DView.CheckState = System.Windows.Forms.CheckState.Checked;
			this.tbtn3DView.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.tbtn3DView.Image = ((System.Drawing.Image)(resources.GetObject("tbtn3DView.Image")));
			this.tbtn3DView.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tbtn3DView.Name = "tbtn3DView";
			this.tbtn3DView.Size = new System.Drawing.Size(25, 22);
			this.tbtn3DView.Text = "3D";
			this.tbtn3DView.CheckedChanged += new System.EventHandler(this.Tbtn3DViewCheckedChanged);
			// 
			// tbtnUVView
			// 
			this.tbtnUVView.CheckOnClick = true;
			this.tbtnUVView.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.tbtnUVView.Image = ((System.Drawing.Image)(resources.GetObject("tbtnUVView.Image")));
			this.tbtnUVView.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tbtnUVView.Name = "tbtnUVView";
			this.tbtnUVView.Size = new System.Drawing.Size(26, 22);
			this.tbtnUVView.Text = "UV";
			this.tbtnUVView.CheckedChanged += new System.EventHandler(this.TbtnUVViewCheckedChanged);
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.fileToolStripMenuItem,
			this.optionsToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(883, 24);
			this.menuStrip1.TabIndex = 3;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.loadToolStripMenuItem,
			this.toolStripSeparator1,
			this.exitToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
			this.fileToolStripMenuItem.Text = "File";
			// 
			// loadToolStripMenuItem
			// 
			this.loadToolStripMenuItem.Name = "loadToolStripMenuItem";
			this.loadToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
			this.loadToolStripMenuItem.Text = "Load";
			this.loadToolStripMenuItem.Click += new System.EventHandler(this.LoadToolStripMenuItemClick);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(97, 6);
			// 
			// exitToolStripMenuItem
			// 
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			this.exitToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
			this.exitToolStripMenuItem.Text = "Exit";
			// 
			// optionsToolStripMenuItem
			// 
			this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.logWindowToolStripMenuItem});
			this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
			this.optionsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
			this.optionsToolStripMenuItem.Text = "Options";
			// 
			// logWindowToolStripMenuItem
			// 
			this.logWindowToolStripMenuItem.CheckOnClick = true;
			this.logWindowToolStripMenuItem.Name = "logWindowToolStripMenuItem";
			this.logWindowToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
			this.logWindowToolStripMenuItem.Text = "Log Window";
			this.logWindowToolStripMenuItem.CheckedChanged += new System.EventHandler(this.LogWindowToolStripMenuItemCheckedChanged);
			// 
			// richTextBox1
			// 
			this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.richTextBox1.Location = new System.Drawing.Point(0, 0);
			this.richTextBox1.Name = "richTextBox1";
			this.richTextBox1.Size = new System.Drawing.Size(150, 46);
			this.richTextBox1.TabIndex = 0;
			this.richTextBox1.Text = "";
			// 
			// CaffFileContextMenu
			// 
			this.CaffFileContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.unloadToolStripMenuItem,
			this.exportToolStripMenuItem});
			this.CaffFileContextMenu.Name = "CaffFileContextMenu";
			this.CaffFileContextMenu.Size = new System.Drawing.Size(113, 48);
			// 
			// unloadToolStripMenuItem
			// 
			this.unloadToolStripMenuItem.Name = "unloadToolStripMenuItem";
			this.unloadToolStripMenuItem.Size = new System.Drawing.Size(112, 22);
			this.unloadToolStripMenuItem.Text = "Unload";
			this.unloadToolStripMenuItem.Click += new System.EventHandler(this.UnloadToolStripMenuItemClick);
			// 
			// exportToolStripMenuItem
			// 
			this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
			this.exportToolStripMenuItem.Size = new System.Drawing.Size(112, 22);
			this.exportToolStripMenuItem.Text = "Export";
			this.exportToolStripMenuItem.Click += new System.EventHandler(this.ExportToolStripMenuItemClick);
			// 
			// vertexBatchContextStrip
			// 
			this.vertexBatchContextStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.setDefaultTexCoordScaleToolStripMenuItem});
			this.vertexBatchContextStrip.Name = "contextMenuStrip1";
			this.vertexBatchContextStrip.Size = new System.Drawing.Size(215, 26);
			// 
			// setDefaultTexCoordScaleToolStripMenuItem
			// 
			this.setDefaultTexCoordScaleToolStripMenuItem.Name = "setDefaultTexCoordScaleToolStripMenuItem";
			this.setDefaultTexCoordScaleToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
			this.setDefaultTexCoordScaleToolStripMenuItem.Text = "Set Default TexCoord Scale";
			this.setDefaultTexCoordScaleToolStripMenuItem.Click += new System.EventHandler(this.SetDefaultTexCoordScaleToolStripMenuItemClick);
			// 
			// textureContextStrip
			// 
			this.textureContextStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.recropToolStripMenuItem,
			this.dumpToXprToolStripMenuItem});
			this.textureContextStrip.Name = "textureContextStrip";
			this.textureContextStrip.Size = new System.Drawing.Size(139, 48);
			// 
			// recropToolStripMenuItem
			// 
			this.recropToolStripMenuItem.Name = "recropToolStripMenuItem";
			this.recropToolStripMenuItem.Size = new System.Drawing.Size(138, 22);
			this.recropToolStripMenuItem.Text = "Recrop";
			this.recropToolStripMenuItem.Click += new System.EventHandler(this.RecropToolStripMenuItemClick);
			// 
			// dumpToXprToolStripMenuItem
			// 
			this.dumpToXprToolStripMenuItem.Name = "dumpToXprToolStripMenuItem";
			this.dumpToXprToolStripMenuItem.Size = new System.Drawing.Size(138, 22);
			this.dumpToXprToolStripMenuItem.Text = "DumpToXpr";
			this.dumpToXprToolStripMenuItem.Click += new System.EventHandler(this.DumpToXprToolStripMenuItemClick);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(883, 533);
			this.Controls.Add(this.splitContainer1);
			this.Name = "MainForm";
			this.Text = "RareView";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainFormFormClosing);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel1.PerformLayout();
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer2.Panel1.ResumeLayout(false);
			this.splitContainer2.Panel2.ResumeLayout(false);
			this.splitContainer2.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
			this.splitContainer2.ResumeLayout(false);
			this.splitContainer3.Panel1.ResumeLayout(false);
			this.splitContainer3.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
			this.splitContainer3.ResumeLayout(false);
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.CaffFileContextMenu.ResumeLayout(false);
			this.vertexBatchContextStrip.ResumeLayout(false);
			this.textureContextStrip.ResumeLayout(false);
			this.ResumeLayout(false);

		}
	}
}
