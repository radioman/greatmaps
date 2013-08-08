using MSR.CVE.BackMaker.ImagePipeline;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
namespace MSR.CVE.BackMaker
{
	public class SourceMapInfoPanel : UserControl
	{
		public delegate void PreviewSourceMapZoomDelegate(SourceMap sourceMap);
		private SourceMap sourceMap;
		private bool needUpdate;
		private SourceMapInfoPanel.PreviewSourceMapZoomDelegate previewSourceMapZoom;
		private IContainer components;
		private Label label1;
		private TextBox mapFileURLTextBox;
		private Label label2;
		private TextBox mapHomePageTextBox;
		private TextBox mapDescriptionTextBox;
		private Panel panel1;
		private Label label3;
		private TextBox textBox7;
		private NumericUpDown closestZoomUpDown;
		public SourceMapInfoPanel()
		{
			this.InitializeComponent();
			this.mapFileURLTextBox.LostFocus += new EventHandler(this.mapFileURLTextBox_LostFocus);
			this.mapHomePageTextBox.LostFocus += new EventHandler(this.mapHomePageTextBox_LostFocus);
			this.mapDescriptionTextBox.LostFocus += new EventHandler(this.descriptionTextBox_LostFocus);
			MercatorCoordinateSystem mercatorCoordinateSystem = new MercatorCoordinateSystem();
			this.closestZoomUpDown.Minimum = mercatorCoordinateSystem.GetZoomRange().min;
			this.closestZoomUpDown.Maximum = mercatorCoordinateSystem.GetZoomRange().max;
		}
		public void Initialize(SourceMapInfoPanel.PreviewSourceMapZoomDelegate previewSourceMapZoom)
		{
			this.previewSourceMapZoom = previewSourceMapZoom;
		}
		private void descriptionTextBox_LostFocus(object sender, EventArgs e)
		{
			if (this.sourceMap != null)
			{
				this.sourceMap.sourceMapInfo.mapDescription = ((TextBox)sender).Text;
			}
		}
		private void mapHomePageTextBox_LostFocus(object sender, EventArgs e)
		{
			if (this.sourceMap != null)
			{
				this.sourceMap.sourceMapInfo.mapHomePage = ((TextBox)sender).Text;
			}
		}
		private void mapFileURLTextBox_LostFocus(object sender, EventArgs e)
		{
			if (this.sourceMap != null)
			{
				this.sourceMap.sourceMapInfo.mapFileURL = ((TextBox)sender).Text;
			}
		}
		public void Configure(SourceMap sourceMap)
		{
			if (this.sourceMap != null)
			{
				this.sourceMap.sourceMapRenderOptions.dirtyEvent.Remove(new DirtyListener(this.ZoomChangedHandler));
			}
			this.sourceMap = sourceMap;
			if (this.sourceMap != null)
			{
				this.sourceMap.sourceMapRenderOptions.dirtyEvent.Add(new DirtyListener(this.ZoomChangedHandler));
			}
			this.update();
		}
		private void update()
		{
			if (this.sourceMap != null)
			{
				this.mapFileURLTextBox.Text = this.sourceMap.sourceMapInfo.mapFileURL;
				this.mapHomePageTextBox.Text = this.sourceMap.sourceMapInfo.mapHomePage;
				this.mapDescriptionTextBox.Text = this.sourceMap.sourceMapInfo.mapDescription;
			}
			else
			{
				this.mapFileURLTextBox.Text = "";
				this.mapHomePageTextBox.Text = "";
				this.mapDescriptionTextBox.Text = "";
			}
			if (this.sourceMap == null || this.sourceMap.sourceMapRenderOptions.maxZoom == -1)
			{
				this.closestZoomUpDown.Value = this.closestZoomUpDown.Minimum;
				this.closestZoomUpDown.Enabled = false;
				return;
			}
			this.closestZoomUpDown.Value = this.sourceMap.sourceMapRenderOptions.maxZoom;
			this.closestZoomUpDown.Enabled = true;
		}
		private void ZoomChangedHandler()
		{
			this.needUpdate = true;
			base.Invalidate();
		}
		private void closestZoomUpDown_ValueChanged(object sender, EventArgs e)
		{
			if (this.sourceMap != null)
			{
				this.sourceMap.sourceMapRenderOptions.maxZoom = Convert.ToInt32(this.closestZoomUpDown.Value);
				this.previewSourceMapZoom(this.sourceMap);
			}
		}
		protected override void OnPaint(PaintEventArgs e)
		{
			if (D.CustomPaintDisabled())
			{
				return;
			}
			if (this.needUpdate)
			{
				this.update();
				this.needUpdate = false;
			}
			base.OnPaint(e);
		}
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
			this.label1 = new Label();
			this.mapFileURLTextBox = new TextBox();
			this.label2 = new Label();
			this.mapHomePageTextBox = new TextBox();
			this.mapDescriptionTextBox = new TextBox();
			this.panel1 = new Panel();
			this.textBox7 = new TextBox();
			this.closestZoomUpDown = new NumericUpDown();
			this.label3 = new Label();
			this.panel1.SuspendLayout();
			((ISupportInitialize)this.closestZoomUpDown).BeginInit();
			base.SuspendLayout();
			this.label1.AutoSize = true;
			this.label1.Location = new Point(3, 0);
			this.label1.Name = "label1";
			this.label1.Size = new Size(75, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Map File URL:";
			this.mapFileURLTextBox.Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
			this.mapFileURLTextBox.Location = new Point(6, 20);
			this.mapFileURLTextBox.Name = "mapFileURLTextBox";
			this.mapFileURLTextBox.Size = new Size(300, 20);
			this.mapFileURLTextBox.TabIndex = 1;
			this.label2.AutoSize = true;
			this.label2.Location = new Point(3, 96);
			this.label2.Name = "label2";
			this.label2.Size = new Size(157, 13);
			this.label2.TabIndex = 0;
			this.label2.Text = "Map description and comments:";
			this.mapHomePageTextBox.Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
			this.mapHomePageTextBox.Location = new Point(6, 68);
			this.mapHomePageTextBox.Name = "mapHomePageTextBox";
			this.mapHomePageTextBox.Size = new Size(300, 20);
			this.mapHomePageTextBox.TabIndex = 1;
			this.mapDescriptionTextBox.Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right);
			this.mapDescriptionTextBox.Location = new Point(6, 116);
			this.mapDescriptionTextBox.Multiline = true;
			this.mapDescriptionTextBox.Name = "mapDescriptionTextBox";
			this.mapDescriptionTextBox.Size = new Size(300, 198);
			this.mapDescriptionTextBox.TabIndex = 1;
			this.panel1.Controls.Add(this.textBox7);
			this.panel1.Controls.Add(this.closestZoomUpDown);
			this.panel1.Controls.Add(this.label3);
			this.panel1.Controls.Add(this.label1);
			this.panel1.Controls.Add(this.mapDescriptionTextBox);
			this.panel1.Controls.Add(this.mapFileURLTextBox);
			this.panel1.Controls.Add(this.mapHomePageTextBox);
			this.panel1.Controls.Add(this.label2);
			this.panel1.Dock = DockStyle.Fill;
			this.panel1.Location = new Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new Size(309, 365);
			this.panel1.TabIndex = 2;
			this.textBox7.Anchor = (AnchorStyles.Bottom | AnchorStyles.Left);
			this.textBox7.BackColor = SystemColors.ControlLight;
			this.textBox7.BorderStyle = BorderStyle.None;
			this.textBox7.Location = new Point(6, 345);
			this.textBox7.Name = "textBox7";
			this.textBox7.Size = new Size(132, 13);
			this.textBox7.TabIndex = 9;
			this.textBox7.TabStop = false;
			this.textBox7.Text = "Maximum (Closest) Zoom";
			this.closestZoomUpDown.Anchor = (AnchorStyles.Bottom | AnchorStyles.Right);
			this.closestZoomUpDown.Location = new Point(260, 342);
			this.closestZoomUpDown.Name = "closestZoomUpDown";
			this.closestZoomUpDown.Size = new Size(46, 20);
			this.closestZoomUpDown.TabIndex = 8;
			this.closestZoomUpDown.ValueChanged += new EventHandler(this.closestZoomUpDown_ValueChanged);
			this.label3.AutoSize = true;
			this.label3.Location = new Point(3, 48);
			this.label3.Name = "label3";
			this.label3.Size = new Size(87, 13);
			this.label3.TabIndex = 2;
			this.label3.Text = "Map home page:";
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = AutoScaleMode.Font;
			base.Controls.Add(this.panel1);
			base.Name = "SourceMapOptions";
			base.Size = new Size(309, 365);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			((ISupportInitialize)this.closestZoomUpDown).EndInit();
			base.ResumeLayout(false);
		}
	}
}
