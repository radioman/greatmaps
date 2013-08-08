using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
namespace MSR.CVE.BackMaker
{
	public class AboutForm : Form
	{
		private IContainer components;
		private WebBrowser aboutContentsBrowser;
		private Label label2;
		private Label label3;
		public AboutForm(string versionNumber)
		{
			this.InitializeComponent();
			string text = string.Concat(new string[]
			{
				"\r\n<html>\r\n<center>\r\n<br /><br /><i>",
				BuildConfig.theConfig.editionName,
				"</i>\r\n<br>Version ",
				versionNumber,
				"\r\n<p>\r\n<a target=\"new\" href=\"",
				BuildConfig.theConfig.mapCruncherHomeSite,
				"\">MapCruncher</a> was\r\ncreated at <a target=\"new\" href=\"http://research.microsoft.com/\">Microsoft Research</a>,<br>by the\r\nComposable Virtual Earth team"
			});
			if (BuildConfig.theConfig.buildConfiguration != "VE")
			{
				text += ":<br>Jeremy Elson, Jon Howell, John Douceur, and Danyel Fisher";
			}
			text += ".\r\n</center>\r\n<p>\r\n<center>&copy; 2007 Microsoft Corporation.  All rights reserved.</center>\r\n<p>\r\n<center>\r\nFoxit PDF Reader SDK\r\n<br>Copyright &copy; 2002-2006, Foxit Software Company\r\n<br><a target=\"new\" href=\"http://www.foxitsoftware.com/\">www.foxitsoftware.com</a>, All rights reserved.\r\n</center>";
			if (BuildConfig.theConfig.buildConfiguration != "VE")
			{
				text += "\r\n<p>\r\n<center>\r\nSpecial thanks to:\r\nJim Blinn,\r\nRich Draves,\r\nSteve Lombardi,\r\nKaren Luecking,\r\nJoe Schwartz,\r\nMarvin Theimer,\r\nChandu Thota,\r\nand everyone who has tested MapCruncher!\r\n</center>";
			}
			//text += "\r\n<p>\r\n<center>Feedback is welcome.  Write to<br>\r\n<tt>cruncher</tt> at <tt>microsoft</tt> dot <tt>com</tt></center>\r\n\r\n<p>\r\n<b>Warning:</b>\r\nThis computer program is protected by copyright law and international treaties.\r\nUnauthorized reproduction or distribution of this program, or any portion of it,\r\nmay result in severe civil and criminal penalties, and will be prosecuted to the\r\nmaximum extent possible under the law.\r\n</html>\r\n";
			this.aboutContentsBrowser.DocumentText = text;
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
			ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(AboutForm));
			this.aboutContentsBrowser = new WebBrowser();
			this.label2 = new Label();
			this.label3 = new Label();
			base.SuspendLayout();
			this.aboutContentsBrowser.Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right);
			this.aboutContentsBrowser.Location = new Point(8, 104);
			this.aboutContentsBrowser.MinimumSize = new Size(20, 20);
			this.aboutContentsBrowser.Name = "aboutContentsBrowser";
			this.aboutContentsBrowser.Size = new Size(558, 439);
			this.aboutContentsBrowser.TabIndex = 3;
			this.label2.Image = (Image)componentResourceManager.GetObject("label2.Image");
			this.label2.Location = new Point(379, 1);
			this.label2.Name = "label2";
			this.label2.Size = new Size(148, 139);
			this.label2.TabIndex = 4;
			this.label3.Image = (Image)componentResourceManager.GetObject("label3.Image");
			this.label3.Location = new Point(33, 22);
			this.label3.Name = "label3";
			this.label3.Size = new Size(340, 92);
			this.label3.TabIndex = 5;
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = AutoScaleMode.Font;
			this.BackColor = SystemColors.ControlLightLight;
			base.ClientSize = new Size(555, 444);
			base.Controls.Add(this.label3);
			base.Controls.Add(this.label2);
			base.Controls.Add(this.aboutContentsBrowser);
			base.Icon = (Icon)componentResourceManager.GetObject("$this.Icon");
			base.Name = "AboutForm";
			base.StartPosition = FormStartPosition.CenterScreen;
			this.Text = "About MapCruncher";
			base.ResumeLayout(false);
		}
	}
}
