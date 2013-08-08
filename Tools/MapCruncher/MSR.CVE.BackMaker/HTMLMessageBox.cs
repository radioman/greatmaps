using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
namespace MSR.CVE.BackMaker
{
	public class HTMLMessageBox : Form
	{
		private IContainer components;
		private WebBrowser webBrowser1;
		private Button OKButton;
		public HTMLMessageBox() : this("Preview of <i>HTML</i> <b>message box</b>.", "Caption Argument")
		{
			this.InitializeComponent();
		}
		public HTMLMessageBox(string htmlContent, string caption)
		{
			this.InitializeComponent();
			this.webBrowser1.DocumentText = htmlContent;
			this.Text = caption;
		}
		public static DialogResult Show(string htmlContent, string caption)
		{
			HTMLMessageBox hTMLMessageBox = new HTMLMessageBox(htmlContent, caption);
			return hTMLMessageBox.ShowDialog();
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
			this.webBrowser1 = new WebBrowser();
			this.OKButton = new Button();
			base.SuspendLayout();
			this.webBrowser1.Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right);
			this.webBrowser1.Location = new Point(0, 0);
			this.webBrowser1.MinimumSize = new Size(20, 20);
			this.webBrowser1.Name = "webBrowser1";
			this.webBrowser1.Size = new Size(339, 254);
			this.webBrowser1.TabIndex = 0;
			this.OKButton.Anchor = (AnchorStyles.Bottom | AnchorStyles.Right);
			this.OKButton.DialogResult = DialogResult.OK;
			this.OKButton.Location = new Point(241, 265);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = new Size(79, 35);
			this.OKButton.TabIndex = 1;
			this.OKButton.Text = "OK";
			this.OKButton.UseVisualStyleBackColor = true;
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = AutoScaleMode.Font;
			base.ClientSize = new Size(339, 311);
			base.Controls.Add(this.OKButton);
			base.Controls.Add(this.webBrowser1);
			base.Name = "HTMLMessageBox";
			this.Text = "HTMLMessageBox";
			base.ResumeLayout(false);
		}
	}
}
