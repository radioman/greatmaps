using MSR.CVE.BackMaker.ImagePipeline;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
namespace MSR.CVE.BackMaker
{
	public class SourceMapOverviewWindow : Form
	{
		public delegate void ClosedDelegate();
		private IContainer components;
		public ViewerControl viewerControl;
		private MapPosition mapPos;
		private SourceMapOverviewWindow.ClosedDelegate closedDelegate;
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
			this.viewerControl = new ViewerControl();
			base.SuspendLayout();
			this.viewerControl.Dock = DockStyle.Fill;
			this.viewerControl.Location = new Point(0, 0);
			this.viewerControl.Name = "viewerControl";
			this.viewerControl.Size = new Size(380, 351);
			this.viewerControl.TabIndex = 0;
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = AutoScaleMode.Font;
			base.ClientSize = new Size(380, 351);
			base.Controls.Add(this.viewerControl);
			base.FormBorderStyle = FormBorderStyle.SizableToolWindow;
			base.Name = "SourceMapOverviewWindow";
			this.Text = "SourceMapOverviewWindow";
			base.TopMost = true;
			base.ResumeLayout(false);
		}
		public SourceMapOverviewWindow()
		{
			this.InitializeComponent();
			this.mapPos = new MapPosition(this.viewerControl);
			this.viewerControl.Initialize(new MapPositionDelegate(this.GetMapPos), "Overview");
		}
		public void Initialize(SourceMapOverviewWindow.ClosedDelegate closedDelegate, MapDrawingOption ShowDMS)
		{
			this.closedDelegate = closedDelegate;
			this.viewerControl.ShowDMS = ShowDMS;
			this.mapPos.setPosition(new ContinuousCoordinateSystem().GetDefaultView());
			base.Closed += new EventHandler(this.SourceMapOverviewWindow_Closed);
		}
		private void SourceMapOverviewWindow_Closed(object sender, EventArgs e)
		{
			this.closedDelegate();
		}
		private MapPosition GetMapPos()
		{
			return this.mapPos;
		}
		private void SetDefaultView()
		{
			this.mapPos.setPosition(this.viewerControl.GetCoordinateSystem().GetDefaultView());
		}
	}
}
