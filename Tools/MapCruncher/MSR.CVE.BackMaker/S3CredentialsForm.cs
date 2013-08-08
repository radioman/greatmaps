using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
namespace MSR.CVE.BackMaker
{
	public class S3CredentialsForm : Form
	{
		private IContainer components;
		private Label label2;
		private Label label5;
		private TextBox s3SecretAccessKey;
		private TextBox s3AccessKeyId;
		private Button saveButton;
		private Button cancelButton;
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
			this.label2 = new Label();
			this.label5 = new Label();
			this.s3SecretAccessKey = new TextBox();
			this.s3AccessKeyId = new TextBox();
			this.saveButton = new Button();
			this.cancelButton = new Button();
			base.SuspendLayout();
			this.label2.AutoSize = true;
			this.label2.Location = new Point(9, 55);
			this.label2.Name = "label2";
			this.label2.Size = new Size(97, 13);
			this.label2.TabIndex = 17;
			this.label2.Text = "Secret Access Key";
			this.label5.AutoSize = true;
			this.label5.Location = new Point(9, 6);
			this.label5.Name = "label5";
			this.label5.Size = new Size(77, 13);
			this.label5.TabIndex = 16;
			this.label5.Text = "Access Key ID";
			this.s3SecretAccessKey.Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
			this.s3SecretAccessKey.Location = new Point(12, 71);
			this.s3SecretAccessKey.Name = "s3SecretAccessKey";
			this.s3SecretAccessKey.Size = new Size(355, 20);
			this.s3SecretAccessKey.TabIndex = 15;
			this.s3AccessKeyId.Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
			this.s3AccessKeyId.Location = new Point(12, 22);
			this.s3AccessKeyId.Name = "s3AccessKeyId";
			this.s3AccessKeyId.Size = new Size(355, 20);
			this.s3AccessKeyId.TabIndex = 14;
			this.saveButton.Anchor = (AnchorStyles.Bottom | AnchorStyles.Right);
			this.saveButton.DialogResult = DialogResult.Yes;
			this.saveButton.Location = new Point(292, 123);
			this.saveButton.Name = "saveButton";
			this.saveButton.Size = new Size(75, 23);
			this.saveButton.TabIndex = 18;
			this.saveButton.Text = "Save";
			this.saveButton.UseVisualStyleBackColor = true;
			this.cancelButton.Anchor = (AnchorStyles.Bottom | AnchorStyles.Right);
			this.cancelButton.DialogResult = DialogResult.Cancel;
			this.cancelButton.Location = new Point(211, 123);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new Size(75, 23);
			this.cancelButton.TabIndex = 18;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = AutoScaleMode.Font;
			base.ClientSize = new Size(379, 158);
			base.Controls.Add(this.cancelButton);
			base.Controls.Add(this.saveButton);
			base.Controls.Add(this.label2);
			base.Controls.Add(this.label5);
			base.Controls.Add(this.s3SecretAccessKey);
			base.Controls.Add(this.s3AccessKeyId);
			base.Name = "S3CredentialsForm";
			this.Text = "S3CredentialsForm";
			base.ResumeLayout(false);
			base.PerformLayout();
		}
		public S3CredentialsForm()
		{
			this.InitializeComponent();
		}
		internal void Initialize(S3Credentials s3c)
		{
			this.s3AccessKeyId.Text = s3c.accessKeyId;
			this.s3SecretAccessKey.Text = s3c.secretAccessKey;
		}
		internal void LoadResult(S3Credentials s3c)
		{
			s3c.accessKeyId = this.s3AccessKeyId.Text;
			s3c.secretAccessKey = this.s3SecretAccessKey.Text;
		}
	}
}
