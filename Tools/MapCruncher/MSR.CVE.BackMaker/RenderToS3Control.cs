using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
namespace MSR.CVE.BackMaker
{
	public class RenderToS3Control : UserControl
	{
		private IContainer components;
		private Label label4;
		private Label label3;
		private Label label5;
		private TextBox s3PathPrefix;
		private TextBox s3Bucket;
		private TextBox s3CredentialsFilename;
		private Button credentialsBrowseButton;
		private Button editButton;
		private ToolTip toolTip1;
		private RenderToS3Options renderToS3Options;
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
			this.label4 = new Label();
			this.label3 = new Label();
			this.label5 = new Label();
			this.s3PathPrefix = new TextBox();
			this.s3Bucket = new TextBox();
			this.s3CredentialsFilename = new TextBox();
			this.credentialsBrowseButton = new Button();
			this.editButton = new Button();
			this.toolTip1 = new ToolTip(this.components);
			base.SuspendLayout();
			this.label4.AutoSize = true;
			this.label4.Location = new Point(3, 92);
			this.label4.Name = "label4";
			this.label4.Size = new Size(58, 13);
			this.label4.TabIndex = 15;
			this.label4.Text = "Path Prefix";
			this.label3.AutoSize = true;
			this.label3.Location = new Point(3, 62);
			this.label3.Name = "label3";
			this.label3.Size = new Size(41, 13);
			this.label3.TabIndex = 14;
			this.label3.Text = "Bucket";
			this.label5.AutoSize = true;
			this.label5.Location = new Point(3, 9);
			this.label5.Name = "label5";
			this.label5.Size = new Size(59, 13);
			this.label5.TabIndex = 12;
			this.label5.Text = "Credentials";
			this.s3PathPrefix.Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
			this.s3PathPrefix.Location = new Point(68, 89);
			this.s3PathPrefix.Name = "s3PathPrefix";
			this.s3PathPrefix.Size = new Size(277, 20);
			this.s3PathPrefix.TabIndex = 11;
			this.s3Bucket.Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
			this.s3Bucket.Location = new Point(68, 59);
			this.s3Bucket.Name = "s3Bucket";
			this.s3Bucket.Size = new Size(277, 20);
			this.s3Bucket.TabIndex = 10;
			this.s3CredentialsFilename.Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
			this.s3CredentialsFilename.Location = new Point(68, 6);
			this.s3CredentialsFilename.Name = "s3CredentialsFilename";
			this.s3CredentialsFilename.Size = new Size(275, 20);
			this.s3CredentialsFilename.TabIndex = 8;
			this.toolTip1.SetToolTip(this.s3CredentialsFilename, "Path to file containing credentials for render upload");
			this.credentialsBrowseButton.Location = new Point(68, 32);
			this.credentialsBrowseButton.Name = "credentialsBrowseButton";
			this.credentialsBrowseButton.Size = new Size(66, 20);
			this.credentialsBrowseButton.TabIndex = 16;
			this.credentialsBrowseButton.Text = "Select...";
			this.toolTip1.SetToolTip(this.credentialsBrowseButton, "Select existing or create new credentials file from browser.");
			this.credentialsBrowseButton.UseVisualStyleBackColor = true;
			this.credentialsBrowseButton.Click += new EventHandler(this.credentialsBrowseButton_Click);
			this.editButton.Location = new Point(140, 32);
			this.editButton.Name = "editButton";
			this.editButton.Size = new Size(66, 20);
			this.editButton.TabIndex = 17;
			this.editButton.Text = "Edit...";
			this.toolTip1.SetToolTip(this.editButton, "Edit this credentials file.");
			this.editButton.UseVisualStyleBackColor = true;
			this.editButton.Click += new EventHandler(this.editButton_Click);
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = AutoScaleMode.Font;
			this.BackColor = SystemColors.ControlLightLight;
			base.Controls.Add(this.editButton);
			base.Controls.Add(this.credentialsBrowseButton);
			base.Controls.Add(this.label4);
			base.Controls.Add(this.label3);
			base.Controls.Add(this.label5);
			base.Controls.Add(this.s3PathPrefix);
			base.Controls.Add(this.s3Bucket);
			base.Controls.Add(this.s3CredentialsFilename);
			base.Name = "RenderToS3Control";
			base.Size = new Size(346, 117);
			base.ResumeLayout(false);
			base.PerformLayout();
		}
		public RenderToS3Control()
		{
			this.InitializeComponent();
			this.s3CredentialsFilename.LostFocus += new EventHandler(this.s3Credentials_LostFocus);
			this.UpdateButtons();
			this.s3Bucket.LostFocus += new EventHandler(this.s3Bucket_LostFocus);
			this.s3PathPrefix.LostFocus += new EventHandler(this.s3PathPrefix_LostFocus);
		}
		private void s3PathPrefix_LostFocus(object sender, EventArgs e)
		{
			this.renderToS3Options.s3pathPrefix = this.s3PathPrefix.Text;
		}
		private void s3Bucket_LostFocus(object sender, EventArgs e)
		{
			this.renderToS3Options.s3bucket = this.s3Bucket.Text;
		}
		private void s3Credentials_LostFocus(object sender, EventArgs e)
		{
			this.renderToS3Options.s3credentialsFilename = this.s3CredentialsFilename.Text;
			this.UpdateButtons();
		}
		private void UpdateButtons()
		{
			this.editButton.Enabled = this.FileIsReadable();
		}
		public void Configure(RenderToS3Options renderToS3Options)
		{
			this.renderToS3Options = renderToS3Options;
			this.Reload();
		}
		private void Reload()
		{
			if (this.renderToS3Options != null)
			{
				this.s3CredentialsFilename.Text = this.renderToS3Options.s3credentialsFilename;
				this.UpdateButtons();
				this.s3Bucket.Text = this.renderToS3Options.s3bucket;
				this.s3PathPrefix.Text = this.renderToS3Options.s3pathPrefix;
			}
		}
		private bool FileIsReadable()
		{
			bool result;
			try
			{
				new S3Credentials(this.renderToS3Options.s3credentialsFilename, false);
				result = true;
			}
			catch (Exception)
			{
				result = false;
			}
			return result;
		}
		private void credentialsBrowseButton_Click(object sender, EventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.SupportMultiDottedExtensions = true;
			openFileDialog.DefaultExt = ".cred.xml";
			openFileDialog.Filter = "Credentials File (*.cred.xml)|*.cred.xml" + BuildConfig.theConfig.allFilesOption;
			openFileDialog.CheckFileExists = false;
			DialogResult dialogResult = openFileDialog.ShowDialog();
			if (dialogResult != DialogResult.OK)
			{
				return;
			}
			this.renderToS3Options.s3credentialsFilename = openFileDialog.FileName;
			if (!this.FileIsReadable())
			{
				this.EditFile();
				if (!this.FileIsReadable())
				{
					this.renderToS3Options.s3credentialsFilename = "";
				}
			}
			this.Reload();
		}
		private void editButton_Click(object sender, EventArgs e)
		{
			this.EditFile();
		}
		private void EditFile()
		{
			S3Credentials s3Credentials = new S3Credentials(this.renderToS3Options.s3credentialsFilename, true);
			S3CredentialsForm s3CredentialsForm = new S3CredentialsForm();
			s3CredentialsForm.Initialize(s3Credentials);
			DialogResult dialogResult = s3CredentialsForm.ShowDialog();
			if (dialogResult == DialogResult.Yes)
			{
				s3CredentialsForm.LoadResult(s3Credentials);
				s3Credentials.WriteXML();
			}
		}
	}
}
