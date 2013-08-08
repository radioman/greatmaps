using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
namespace MSR.CVE.BackMaker
{
	public class RenderOptionsPanel : UserControl
	{
		private IContainer components;
		private CheckBox publishSourcesCheckbox;
		private TextBox publishSourcesLabel;
		private ToolTip publishSourceMapsTip;
		private Panel panel1;
		private CheckBox permitCompositionCheckbox;
		private RadioButton renderToS3radio;
		private Label label1;
		private RadioButton renderToFileRadio;
		private bool needReload;
		private RenderOptions renderOptions;
		private Control renderToControl;
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
			this.components = new Container();
			this.publishSourcesCheckbox = new CheckBox();
			this.publishSourcesLabel = new TextBox();
			this.publishSourceMapsTip = new ToolTip(this.components);
			this.permitCompositionCheckbox = new CheckBox();
			this.panel1 = new Panel();
			this.renderToFileRadio = new RadioButton();
			this.label1 = new Label();
			this.renderToS3radio = new RadioButton();
			this.panel1.SuspendLayout();
			base.SuspendLayout();
			this.publishSourcesCheckbox.AutoSize = true;
			this.publishSourcesCheckbox.Location = new Point(3, 198);
			this.publishSourcesCheckbox.Name = "publishSourcesCheckbox";
			this.publishSourcesCheckbox.Size = new Size(15, 14);
			this.publishSourcesCheckbox.TabIndex = 15;
			this.publishSourceMapsTip.SetToolTip(this.publishSourcesCheckbox, "Provides site visitors with all of the data needed to re-render your crunchup.");
			this.publishSourcesCheckbox.UseVisualStyleBackColor = true;
			this.publishSourcesCheckbox.CheckedChanged += new EventHandler(this.publishSourcesCheckbox_CheckedChanged);
			this.publishSourcesLabel.Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
			this.publishSourcesLabel.BackColor = SystemColors.ControlLightLight;
			this.publishSourcesLabel.BorderStyle = BorderStyle.None;
			this.publishSourcesLabel.Location = new Point(24, 198);
			this.publishSourcesLabel.Multiline = true;
			this.publishSourcesLabel.Name = "publishSourcesLabel";
			this.publishSourcesLabel.ReadOnly = true;
			this.publishSourcesLabel.Size = new Size(305, 35);
			this.publishSourcesLabel.TabIndex = 16;
			this.publishSourcesLabel.Text = "Copy source maps and crunchup data to output folder";
			this.publishSourceMapsTip.SetToolTip(this.publishSourcesLabel, "Provides site visitors with all of the data needed to re-render your crunchup.");
			this.publishSourcesLabel.TextChanged += new EventHandler(this.textBox1_TextChanged);
			this.publishSourceMapsTip.Popup += new PopupEventHandler(this.publishSourceMapsTip_Popup);
			this.permitCompositionCheckbox.AutoSize = true;
			this.permitCompositionCheckbox.Location = new Point(3, 239);
			this.permitCompositionCheckbox.Name = "permitCompositionCheckbox";
			this.permitCompositionCheckbox.Size = new Size(114, 17);
			this.permitCompositionCheckbox.TabIndex = 15;
			this.permitCompositionCheckbox.Text = "Permit composition";
			this.permitCompositionCheckbox.UseVisualStyleBackColor = true;
			this.permitCompositionCheckbox.CheckedChanged += new EventHandler(this.permitCompositionCheckbox_CheckedChanged);
			this.panel1.BackColor = SystemColors.ControlLightLight;
			this.panel1.Controls.Add(this.renderToS3radio);
			this.panel1.Controls.Add(this.label1);
			this.panel1.Controls.Add(this.renderToFileRadio);
			this.panel1.Controls.Add(this.publishSourcesLabel);
			this.panel1.Controls.Add(this.permitCompositionCheckbox);
			this.panel1.Controls.Add(this.publishSourcesCheckbox);
			this.panel1.Dock = DockStyle.Fill;
			this.panel1.Location = new Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new Size(332, 362);
			this.panel1.TabIndex = 18;
			this.panel1.Paint += new PaintEventHandler(this.panel1_Paint);
			this.renderToFileRadio.AutoSize = true;
			this.renderToFileRadio.Location = new Point(19, 23);
			this.renderToFileRadio.Name = "renderToFileRadio";
			this.renderToFileRadio.Size = new Size(41, 17);
			this.renderToFileRadio.TabIndex = 18;
			this.renderToFileRadio.TabStop = true;
			this.renderToFileRadio.Text = "File";
			this.renderToFileRadio.UseVisualStyleBackColor = true;
			this.renderToFileRadio.CheckedChanged += new EventHandler(this.renderToFileRadio_CheckedChanged);
			this.label1.AutoSize = true;
			this.label1.Location = new Point(3, 7);
			this.label1.Name = "label1";
			this.label1.Size = new Size(57, 13);
			this.label1.TabIndex = 19;
			this.label1.Text = "Render to:";
			this.label1.Click += new EventHandler(this.label1_Click);
			this.renderToS3radio.AutoSize = true;
			this.renderToS3radio.Location = new Point(79, 23);
			this.renderToS3radio.Name = "renderToS3radio";
			this.renderToS3radio.Size = new Size(38, 17);
			this.renderToS3radio.TabIndex = 20;
			this.renderToS3radio.TabStop = true;
			this.renderToS3radio.Text = "S3";
			this.renderToS3radio.UseVisualStyleBackColor = true;
			this.renderToS3radio.CheckedChanged += new EventHandler(this.renderToS3radio_CheckedChanged);
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = AutoScaleMode.Font;
			this.BackColor = SystemColors.Control;
			base.Controls.Add(this.panel1);
			base.Name = "RenderOptionsPanel";
			base.Size = new Size(332, 362);
			this.publishSourceMapsTip.SetToolTip(this, "Use the Render tab after the maps are locked to create your mashup.");
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			base.ResumeLayout(false);
		}
		public RenderOptionsPanel()
		{
			this.InitializeComponent();
			this.publishSourceMapsTip.SetToolTip(this.permitCompositionCheckbox, "Places the " + CrunchedFile.CrunchedFilename + " file into the public domain so that it may be composed with other map applications.");
			string caption = "Provides site visitors with all of the data needed to re-render your crunchup.";
			this.publishSourceMapsTip.SetToolTip(this.publishSourcesCheckbox, caption);
			this.publishSourceMapsTip.SetToolTip(this.publishSourcesLabel, caption);
		}
		public void SetRenderOptions(RenderOptions renderOptions)
		{
			if (!BuildConfig.theConfig.enableS3)
			{
				this.renderToFileRadio.Visible = false;
				this.renderToS3radio.Visible = false;
			}
			if (this.renderOptions != null)
			{
				this.renderOptions.dirtyEvent.Remove(new DirtyListener(this.PromptReload));
			}
			this.renderOptions = renderOptions;
			if (this.renderOptions != null)
			{
				this.renderOptions.dirtyEvent.Add(new DirtyListener(this.PromptReload));
			}
			this.PromptReload();
		}
		protected override void OnPaint(PaintEventArgs e)
		{
			if (D.CustomPaintDisabled())
			{
				return;
			}
			if (this.needReload)
			{
				this.Reload();
			}
			base.OnPaint(e);
		}
		private void PromptReload()
		{
			this.needReload = true;
			base.Invalidate();
		}
		private void Reload()
		{
			this.needReload = false;
			base.Enabled = (this.renderOptions != null);
			if (this.renderOptions != null)
			{
				this.publishSourcesCheckbox.Checked = this.renderOptions.publishSourceData;
				this.permitCompositionCheckbox.Checked = this.renderOptions.permitComposition;
				this.renderToFileRadio.Checked = (this.renderOptions.renderToOptions is RenderToFileOptions);
				this.renderToS3radio.Checked = (this.renderOptions.renderToOptions is RenderToS3Options);
				if (this.renderOptions.renderToOptions is RenderToFileOptions && !(this.renderToControl is RenderToFileControl))
				{
					this.destroyRenderToControl();
					RenderToFileControl renderToFileControl = new RenderToFileControl();
					renderToFileControl.Configure((RenderToFileOptions)this.renderOptions.renderToOptions);
					renderToFileControl.Location = new Point(0, 46);
					this.renderToControl = renderToFileControl;
				}
				if (this.renderOptions.renderToOptions is RenderToS3Options && !(this.renderToControl is RenderToS3Control))
				{
					this.destroyRenderToControl();
					RenderToS3Control renderToS3Control = new RenderToS3Control();
					renderToS3Control.Configure((RenderToS3Options)this.renderOptions.renderToOptions);
					renderToS3Control.Location = new Point(0, 46);
					this.renderToControl = renderToS3Control;
				}
				this.renderToControl.Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
				this.renderToControl.Size = new Size(this.panel1.Width, this.renderToControl.Height);
				this.panel1.Controls.Add(this.renderToControl);
			}
		}
		private void destroyRenderToControl()
		{
			if (this.renderToControl != null)
			{
				this.renderToControl.Dispose();
				this.renderToControl = null;
			}
		}
		private void publishSourcesCheckbox_CheckedChanged(object sender, EventArgs e)
		{
			this.renderOptions.publishSourceData = ((CheckBox)sender).Checked;
		}
		private void textBox1_TextChanged(object sender, EventArgs e)
		{
			this.publishSourcesCheckbox_CheckedChanged(sender, e);
		}
		private void permitCompositionCheckbox_CheckedChanged(object sender, EventArgs e)
		{
			this.renderOptions.permitComposition = ((CheckBox)sender).Checked;
		}
		private void publishSourceMapsTip_Popup(object sender, PopupEventArgs e)
		{
		}
		private void panel1_Paint(object sender, PaintEventArgs e)
		{
		}
		private void label1_Click(object sender, EventArgs e)
		{
		}
		private void renderToFileRadio_CheckedChanged(object sender, EventArgs e)
		{
			if (this.renderToFileRadio.Checked && !(this.renderOptions.renderToOptions is RenderToFileOptions))
			{
				this.renderOptions.renderToOptions = new RenderToFileOptions(this.renderOptions.dirtyEvent);
			}
		}
		private void renderToS3radio_CheckedChanged(object sender, EventArgs e)
		{
			if (this.renderToS3radio.Checked && !(this.renderOptions.renderToOptions is RenderToS3Options))
			{
				this.renderOptions.renderToOptions = new RenderToS3Options(this.renderOptions.dirtyEvent);
			}
		}
	}
}
