using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
namespace MSR.CVE.BackMaker
{
	public class RecoverAutoSavedFileDialog : Form
	{
		private IContainer components;
		private TextBox message;
		private Button openAsNewButton;
		private Button deleteBackupButton;
		private Button cancelButton;
		private NotifyIcon notifyIcon1;
		public RecoverAutoSavedFileDialog()
		{
			this.InitializeComponent();
		}
		public void Initialize(string filename)
		{
			List<string> list = new List<string>();
			list.Add(string.Format("An automatically-saved backup for {0} was found.", filename));
			list.Add("You may:");
			list.Add("    open it as a new mashup,");
			list.Add("    delete it and continue opening the original file, or");
			list.Add("    cancel the open operation.");
			this.message.Lines = list.ToArray();
			this.message.SelectionStart = 0;
			this.message.SelectionLength = 0;
		}
		private void openAsNewButton_Click(object sender, EventArgs e)
		{
			base.DialogResult = DialogResult.Yes;
			base.Close();
		}
		private void deleteBackupButton_Click(object sender, EventArgs e)
		{
			base.DialogResult = DialogResult.Ignore;
			base.Close();
		}
		private void cancelButton_Click(object sender, EventArgs e)
		{
			base.DialogResult = DialogResult.Cancel;
			base.Close();
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
			this.components = new Container();
			this.message = new TextBox();
			this.openAsNewButton = new Button();
			this.deleteBackupButton = new Button();
			this.cancelButton = new Button();
			this.notifyIcon1 = new NotifyIcon(this.components);
			base.SuspendLayout();
			this.message.Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right);
			this.message.BorderStyle = BorderStyle.None;
			this.message.Font = new Font("Microsoft Sans Serif", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
			this.message.Location = new Point(8, 7);
			this.message.Multiline = true;
			this.message.Name = "message";
			this.message.ReadOnly = true;
			this.message.Size = new Size(376, 173);
			this.message.TabIndex = 0;
			this.message.Text = "Example text";
			this.openAsNewButton.Anchor = (AnchorStyles.Bottom | AnchorStyles.Left);
			this.openAsNewButton.Location = new Point(20, 218);
			this.openAsNewButton.Name = "openAsNewButton";
			this.openAsNewButton.Size = new Size(130, 29);
			this.openAsNewButton.TabIndex = 1;
			this.openAsNewButton.Text = "Open Backup as New";
			this.openAsNewButton.UseVisualStyleBackColor = true;
			this.openAsNewButton.Click += new EventHandler(this.openAsNewButton_Click);
			this.deleteBackupButton.Anchor = (AnchorStyles.Bottom | AnchorStyles.Left);
			this.deleteBackupButton.Location = new Point(156, 218);
			this.deleteBackupButton.Name = "deleteBackupButton";
			this.deleteBackupButton.Size = new Size(107, 29);
			this.deleteBackupButton.TabIndex = 1;
			this.deleteBackupButton.Text = "Delete Backup";
			this.deleteBackupButton.UseVisualStyleBackColor = true;
			this.deleteBackupButton.Click += new EventHandler(this.deleteBackupButton_Click);
			this.cancelButton.Anchor = (AnchorStyles.Bottom | AnchorStyles.Left);
			this.cancelButton.Location = new Point(269, 218);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new Size(107, 29);
			this.cancelButton.TabIndex = 1;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new EventHandler(this.cancelButton_Click);
			this.notifyIcon1.Text = "notifyIcon1";
			this.notifyIcon1.Visible = true;
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = AutoScaleMode.Font;
			base.ClientSize = new Size(391, 266);
			base.Controls.Add(this.cancelButton);
			base.Controls.Add(this.deleteBackupButton);
			base.Controls.Add(this.openAsNewButton);
			base.Controls.Add(this.message);
			base.Name = "RecoverAutoSavedFileDialog";
			this.Text = "Recover automatically-saved backup file";
			base.ResumeLayout(false);
			base.PerformLayout();
		}
	}
}
