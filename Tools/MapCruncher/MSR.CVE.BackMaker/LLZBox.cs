using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
namespace MSR.CVE.BackMaker
{
	public class LLZBox : UserControl, InvalidatableViewIfc
	{
		public enum LabelStyle
		{
			LatLon,
			XY
		}
		private DegreesMinutesSeconds dms = new DegreesMinutesSeconds();
		private MapDrawingOption _ShowDMS;
		private LatLonEditIfc latLonEdit;
		private LatLonZoom lastValue;
		private IContainer components;
		private GroupBox groupBox;
		private Label zoomLabel;
		private Label zoomLabel_text;
		private Label lonLabel_text;
		private Label latLabel_text;
		private TextBox lonText;
		private TextBox latText;
		public MapDrawingOption ShowDMS
		{
			set
			{
				this._ShowDMS = value;
				this._ShowDMS.SetInvalidatableView(this);
			}
		}
		public LLZBox()
		{
			this.InitializeComponent();
			this.latText.LostFocus += new EventHandler(this.latText_TextChanged);
			this.lonText.LostFocus += new EventHandler(this.lonText_TextChanged);
		}
		private void latText_TextChanged(object sender, EventArgs e)
		{
			try
			{
				double newLat = this.dms.ParseLatLon(this.latText.Text);
				this.latLonEdit.latEdited(newLat);
			}
			catch
			{
				this.PositionChanged(this.lastValue);
			}
		}
		private void lonText_TextChanged(object sender, EventArgs e)
		{
			try
			{
				double newLon = this.dms.ParseLatLon(this.lonText.Text);
				this.latLonEdit.lonEdited(newLon);
			}
			catch
			{
				this.PositionChanged(this.lastValue);
			}
		}
		public void configureEditable(LatLonEditIfc latLonEdit)
		{
			this.latLonEdit = latLonEdit;
			this.latText.ReadOnly = false;
			this.lonText.ReadOnly = false;
		}
		public void setName(string name)
		{
			this.groupBox.Text = name;
		}
		public void PositionChanged(LatLonZoom llz)
		{
			this.lastValue = llz;
			this.InvalidateView();
		}
		public void InvalidateView()
		{
			this.dms.outputMode = (this._ShowDMS.Enabled ? DegreesMinutesSeconds.OutputMode.DMS : DegreesMinutesSeconds.OutputMode.DecimalDegrees);
			this.latText.Text = this.dms.FormatLatLon(this.lastValue.lat);
			this.lonText.Text = this.dms.FormatLatLon(this.lastValue.lon);
			this.zoomLabel.Text = this.lastValue.zoom.ToString();
		}
		public void SetLabelStyle(LLZBox.LabelStyle labelStyle)
		{
			switch (labelStyle)
			{
			case LLZBox.LabelStyle.LatLon:
				this.latLabel_text.Text = "Latitude";
				this.lonLabel_text.Text = "Longitude";
				return;
			case LLZBox.LabelStyle.XY:
				this.latLabel_text.Text = "Y";
				this.lonLabel_text.Text = "X";
				return;
			default:
				return;
			}
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
			this.groupBox = new GroupBox();
			this.zoomLabel = new Label();
			this.zoomLabel_text = new Label();
			this.lonLabel_text = new Label();
			this.latLabel_text = new Label();
			this.latText = new TextBox();
			this.lonText = new TextBox();
			this.groupBox.SuspendLayout();
			base.SuspendLayout();
			this.groupBox.Controls.Add(this.lonText);
			this.groupBox.Controls.Add(this.latText);
			this.groupBox.Controls.Add(this.zoomLabel);
			this.groupBox.Controls.Add(this.zoomLabel_text);
			this.groupBox.Controls.Add(this.lonLabel_text);
			this.groupBox.Controls.Add(this.latLabel_text);
			this.groupBox.Location = new Point(3, 3);
			this.groupBox.Name = "groupBox";
			this.groupBox.Size = new Size(175, 74);
			this.groupBox.TabIndex = 10;
			this.groupBox.TabStop = false;
			this.groupBox.Text = "Map Location";
			this.zoomLabel.AutoSize = true;
			this.zoomLabel.Location = new Point(76, 56);
			this.zoomLabel.Name = "zoomLabel";
			this.zoomLabel.Size = new Size(13, 13);
			this.zoomLabel.TabIndex = 5;
			this.zoomLabel.Text = "0";
			this.zoomLabel_text.AutoSize = true;
			this.zoomLabel_text.Location = new Point(7, 56);
			this.zoomLabel_text.Name = "zoomLabel_text";
			this.zoomLabel_text.Size = new Size(63, 13);
			this.zoomLabel_text.TabIndex = 2;
			this.zoomLabel_text.Text = "Zoom Level";
			this.lonLabel_text.AutoSize = true;
			this.lonLabel_text.Location = new Point(7, 33);
			this.lonLabel_text.Name = "lonLabel_text";
			this.lonLabel_text.Size = new Size(54, 13);
			this.lonLabel_text.TabIndex = 1;
			this.lonLabel_text.Text = "Longitude";
			this.latLabel_text.AutoSize = true;
			this.latLabel_text.Location = new Point(7, 16);
			this.latLabel_text.Name = "latLabel_text";
			this.latLabel_text.Size = new Size(45, 13);
			this.latLabel_text.TabIndex = 0;
			this.latLabel_text.Text = "Latitude";
			this.latText.Location = new Point(69, 13);
			this.latText.Name = "latText";
			this.latText.ReadOnly = true;
			this.latText.Size = new Size(100, 20);
			this.latText.TabIndex = 6;
			this.lonText.Location = new Point(69, 33);
			this.lonText.Name = "lonText";
			this.lonText.ReadOnly = true;
			this.lonText.Size = new Size(100, 20);
			this.lonText.TabIndex = 7;
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = AutoScaleMode.Font;
			base.Controls.Add(this.groupBox);
			base.Name = "LLZBox";
			base.Size = new Size(181, 80);
			this.groupBox.ResumeLayout(false);
			this.groupBox.PerformLayout();
			base.ResumeLayout(false);
		}
	}
}
