using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
namespace MSR.CVE.BackMaker
{
	public class RenderToFileControl : UserControl
	{
		private RenderToFileOptions renderToFileOptions;
		private IContainer components;
		private TextBox textBox9;
		private Button selectOutputFolderButton;
		private TextBox outputFolderDisplayBox;
		public RenderToFileControl()
		{
			this.InitializeComponent();
			this.outputFolderDisplayBox.LostFocus += new EventHandler(this.outputFolderDisplayBox_LostFocus);
		}
		public void Configure(RenderToFileOptions renderToFileOptions)
		{
			this.renderToFileOptions = renderToFileOptions;
			this.Reload();
		}
		private void outputFolderDisplayBox_LostFocus(object sender, EventArgs e)
		{
			this.renderToFileOptions.outputFolder = this.outputFolderDisplayBox.Text;
		}
		private void selectOutputFolderButton_Click(object sender, EventArgs e)
		{
			FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
			folderBrowserDialog.ShowNewFolderButton = true;
			folderBrowserDialog.Description = "Select Output Folder";
			DialogResult dialogResult = folderBrowserDialog.ShowDialog();
			if (dialogResult == DialogResult.OK)
			{
				this.renderToFileOptions.outputFolder = folderBrowserDialog.SelectedPath;
				this.Reload();
			}
		}
		private void Reload()
		{
			if (this.renderToFileOptions != null)
			{
				this.outputFolderDisplayBox.Text = this.renderToFileOptions.outputFolder;
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
			this.textBox9 = new TextBox();
			this.selectOutputFolderButton = new Button();
			this.outputFolderDisplayBox = new TextBox();
			base.SuspendLayout();
			this.textBox9.BackColor = SystemColors.ControlLightLight;
			this.textBox9.BorderStyle = BorderStyle.None;
			this.textBox9.Location = new Point(3, 3);
			this.textBox9.Name = "textBox9";
			this.textBox9.Size = new Size(67, 13);
			this.textBox9.TabIndex = 12;
			this.textBox9.TabStop = false;
			this.textBox9.Text = "Output Folder";
			this.selectOutputFolderButton.Anchor = (AnchorStyles.Top | AnchorStyles.Right);
			this.selectOutputFolderButton.Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			this.selectOutputFolderButton.Location = new Point(247, 3);
			this.selectOutputFolderButton.Name = "selectOutputFolderButton";
			this.selectOutputFolderButton.Size = new Size(38, 20);
			this.selectOutputFolderButton.TabIndex = 13;
			this.selectOutputFolderButton.TabStop = false;
			this.selectOutputFolderButton.Text = " ...";
			this.selectOutputFolderButton.TextAlign = ContentAlignment.TopCenter;
			this.selectOutputFolderButton.UseVisualStyleBackColor = true;
			this.selectOutputFolderButton.Click += new EventHandler(this.selectOutputFolderButton_Click);
			this.outputFolderDisplayBox.Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
			this.outputFolderDisplayBox.Location = new Point(3, 29);
			this.outputFolderDisplayBox.Name = "outputFolderDisplayBox";
			this.outputFolderDisplayBox.Size = new Size(282, 20);
			this.outputFolderDisplayBox.TabIndex = 11;
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = AutoScaleMode.Font;
			this.BackColor = SystemColors.ControlLightLight;
			base.Controls.Add(this.textBox9);
			base.Controls.Add(this.selectOutputFolderButton);
			base.Controls.Add(this.outputFolderDisplayBox);
			base.Name = "RenderToFileControl";
			base.Size = new Size(288, 59);
			base.ResumeLayout(false);
			base.PerformLayout();
		}
	}
}
