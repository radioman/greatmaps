using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
namespace MSR.CVE.BackMaker
{
	public class RenderWindow : Form
	{
		private IContainer components;
		private RenderOptionsPanel renderOptionsPanel;
		private RenderProgressPanel2 renderProgressPanel;
		private GroupBox groupBox1;
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}
		private void InitializeComponent()
		{
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.renderOptionsPanel = new MSR.CVE.BackMaker.RenderOptionsPanel();
            this.renderProgressPanel = new MSR.CVE.BackMaker.RenderProgressPanel2();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox1.BackColor = System.Drawing.SystemColors.Control;
            this.groupBox1.Controls.Add(this.renderOptionsPanel);
            this.groupBox1.Location = new System.Drawing.Point(2, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(272, 542);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Mashup Render Options";
            // 
            // renderOptionsPanel
            // 
            this.renderOptionsPanel.BackColor = System.Drawing.SystemColors.Control;
            this.renderOptionsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.renderOptionsPanel.Location = new System.Drawing.Point(3, 18);
            this.renderOptionsPanel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.renderOptionsPanel.Name = "renderOptionsPanel";
            this.renderOptionsPanel.Size = new System.Drawing.Size(266, 521);
            this.renderOptionsPanel.TabIndex = 0;
            // 
            // renderProgressPanel
            // 
            this.renderProgressPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.renderProgressPanel.BackColor = System.Drawing.SystemColors.Control;
            this.renderProgressPanel.Location = new System.Drawing.Point(280, 12);
            this.renderProgressPanel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.renderProgressPanel.Name = "renderProgressPanel";
            this.renderProgressPanel.Size = new System.Drawing.Size(799, 533);
            this.renderProgressPanel.TabIndex = 1;
            // 
            // RenderWindow
            // 
            this.ClientSize = new System.Drawing.Size(1081, 547);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.renderProgressPanel);
            this.Name = "RenderWindow";
            this.Text = "Render";
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

		}
		public RenderWindow()
		{
			this.InitializeComponent();
			base.FormClosed += new FormClosedEventHandler(this.RenderWindow_FormClosed);
		}
		private void RenderWindow_FormClosed(object sender, FormClosedEventArgs e)
		{
			this.UndoConstruction();
		}
		internal void UndoConstruction()
		{
			this.renderOptionsPanel.SetRenderOptions(null);
			this.renderProgressPanel.UndoConstruction();
			base.Dispose();
		}
		internal void Setup(RenderOptions renderOptions, Mashup currentMashup, MapTileSourceFactory mapTileSourceFactory, RenderProgressPanel2.LaunchRenderedBrowserDelegate LaunchRenderedBrowser, RenderState.FlushRenderedTileCachePackageDelegate flushRenderedTileCachePackage)
		{
			this.renderOptionsPanel.SetRenderOptions(renderOptions);
			this.renderProgressPanel.Setup(currentMashup, mapTileSourceFactory, LaunchRenderedBrowser, flushRenderedTileCachePackage);
		}
		internal void StartRender(RenderProgressPanel2.RenderCompleteDelegate renderCompleteDelegate)
		{
			this.renderProgressPanel.StartRender(renderCompleteDelegate);
		}
	}
}
