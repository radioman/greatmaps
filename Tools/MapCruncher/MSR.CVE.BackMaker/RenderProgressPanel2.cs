using MSR.CVE.BackMaker.ImagePipeline;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
namespace MSR.CVE.BackMaker
{
	public class RenderProgressPanel2 : UserControl, RenderUIIfc
	{
		public delegate void LaunchRenderedBrowserDelegate(Uri path);
		public delegate void RenderCompleteDelegate(Exception failure);
		public delegate void NotifyDelegate();
		private Mashup mashup;
		private MapTileSourceFactory mapTileSourceFactory;
		private RenderProgressPanel2.LaunchRenderedBrowserDelegate launchRenderedBrowser;
		private RenderState.FlushRenderedTileCachePackageDelegate flushRenderedTileCachePackage;
		private RenderState renderState;
		private RenderProgressPanel2.RenderCompleteDelegate renderCompleteDelegate;
		private bool updateRequired;
		private ImageRef previewImage;
		private IContainer components;
		private Panel tileDisplayPanel;
		private TextBox currentTileName;
		private TextBox estimatedOutputSizeBox;
		private TextBox textBox1;
		private TextBox renderErrors;
		private LinkLabel previewRenderedResultsLinkLabel;
		private LinkLabel viewInBrowserLinkLabel;
		private Panel panel1;
		private ProgressBar renderProgressBar;
		private Button renderControlButton;
		private TextBox tileDisplayLabel;
		public RenderProgressPanel2()
		{
			this.InitializeComponent();
			this.previewRenderedResultsLinkLabel.Click += new EventHandler(this.previewRenderedResultsLinkLabel_Click);
			base.VisibleChanged += new EventHandler(this.RenderProgressPanel2_VisibleChanged);
		}
		private void RenderProgressPanel2_VisibleChanged(object sender, EventArgs e)
		{
			base.Invalidate();
		}
		public void Setup(Mashup mashup, MapTileSourceFactory mapTileSourceFactory, RenderProgressPanel2.LaunchRenderedBrowserDelegate launchRenderedBrowser, RenderState.FlushRenderedTileCachePackageDelegate flushRenderedTileCachePackage)
		{
			this.flushRenderedTileCachePackage = flushRenderedTileCachePackage;
			this.ReplacePreviewImage(null);
			if (this.mashup != null)
			{
				this.mashup.dirtyEvent.Remove(new DirtyListener(this.MashupChangedHandler));
			}
			this.mashup = mashup;
			this.mapTileSourceFactory = mapTileSourceFactory;
			this.launchRenderedBrowser = launchRenderedBrowser;
			if (this.mashup != null)
			{
				this.mashup.dirtyEvent.Add(new DirtyListener(this.MashupChangedHandler));
			}
			this.MashupChangedHandler();
		}
		private void CheckForUpdate()
		{
			if (this.updateRequired && this.renderState != null)
			{
				this.updateRequired = false;
				this.renderState.UI_UpdateRenderControlButtonLabel(this.renderControlButton);
				this.estimatedOutputSizeBox.Text = this.renderState.UI_GetStatusString();
				this.renderErrors.Lines = this.renderState.UI_GetPostedMessages().ToArray();
				this.renderErrors.SelectionStart = this.renderErrors.Text.Length;
				this.renderErrors.SelectionLength = 0;
				this.renderErrors.ScrollToCaret();
				this.renderState.UI_UpdateProgress(this.renderProgressBar);
				this.renderState.UI_UpdateLinks(this.previewRenderedResultsLinkLabel, this.viewInBrowserLinkLabel);
				this.ReplacePreviewImage(this.renderState.UI_GetLastRenderedImageRef());
				this.tileDisplayLabel.Lines = this.renderState.UI_GetTileDisplayLabel();
			}
		}
		protected override void OnPaint(PaintEventArgs e)
		{
			if (D.CustomPaintDisabled())
			{
				return;
			}
			this.CheckForUpdate();
			base.OnPaint(e);
		}
		private void MashupChangedHandler()
		{
			RenderState renderState = new RenderState(this.mashup, this, this.flushRenderedTileCachePackage, this.mapTileSourceFactory);
			if (!renderState.Equals(this.renderState))
			{
				if (this.renderState != null)
				{
					this.renderState.Dispose();
					this.renderState = null;
				}
				this.renderErrors.Text = "";
				this.renderState = renderState;
				D.Sayf(0, "RenderProgressPanel2: renderState replaced.", new object[0]);
				this.uiChanged();
				return;
			}
			renderState.Dispose();
		}
		private void ReplacePreviewImage(ImageRef newImage)
		{
			if (this.previewImage != null)
			{
				this.previewImage.Dispose();
			}
			this.previewImage = newImage;
		}
		private void renderControlButton_Click(object sender, EventArgs e)
		{
			this.renderState.RenderClick();
		}
		public void StartRender(RenderProgressPanel2.RenderCompleteDelegate renderCompleteDelegate)
		{
			this.renderCompleteDelegate = renderCompleteDelegate;
			this.renderState.StartRender();
		}
		private void tileDisplayPanel_Paint(object sender, PaintEventArgs e)
		{
			Monitor.Enter(this);
			try
			{
				if (this.previewImage != null)
				{
					e.Graphics.FillRectangle(new SolidBrush(Color.LightPink), new Rectangle(new Point(0, 0), this.tileDisplayPanel.Size));
					try
					{
						GDIBigLockedImage image;
						Monitor.Enter(image = this.previewImage.image);
						try
						{
							Image image2 = this.previewImage.image.IPromiseIAmHoldingGDISLockSoPleaseGiveMeTheImage();
							e.Graphics.DrawImage(image2, new Rectangle(new Point(0, 0), this.tileDisplayPanel.Size), new Rectangle(0, 0, image2.Width, image2.Height), GraphicsUnit.Pixel);
						}
						finally
						{
							Monitor.Exit(image);
						}
						goto IL_EB;
					}
					catch (Exception)
					{
						D.Say(0, "Absorbing that disturbing bug wherein the mostRecentTile image is corrupt.");
						goto IL_EB;
					}
				}
				e.Graphics.DrawRectangle(new Pen(Color.Black), 0, 0, this.tileDisplayPanel.Size.Width - 1, this.tileDisplayPanel.Height - 1);
				IL_EB:;
			}
			finally
			{
				Monitor.Exit(this);
			}
		}
		private void previewRenderedResultsLinkLabel_Click(object sender, EventArgs e)
		{
			this.launchRenderedBrowser(this.renderState.GetRenderedXMLDescriptor());
		}
		private void viewInBrowserLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start(new ProcessStartInfo(this.renderState.GetSampleHTMLUri().ToString())
			{
				WindowStyle = ProcessWindowStyle.Normal
			});
		}
		internal void UndoConstruction()
		{
			if (this.mashup != null)
			{
				this.mashup.dirtyEvent.Remove(new DirtyListener(this.MashupChangedHandler));
				this.mashup = null;
			}
			this.renderState.Dispose();
			this.renderState = null;
		}
		public void uiChanged()
		{
			this.updateRequired = true;
			base.Invalidate();
			this.tileDisplayPanel.Invalidate();
		}
		public void notifyRenderComplete(Exception failure)
		{
			if (this.renderCompleteDelegate != null)
			{
				this.renderCompleteDelegate(failure);
				return;
			}
			if (failure == null)
			{
				RenderProgressPanel2.NotifyDelegate method = new RenderProgressPanel2.NotifyDelegate(this.ModalNotifyRenderComplete);
				base.Invoke(method);
			}
		}
		private void ModalNotifyRenderComplete()
		{
			MessageBox.Show(this, "Render completed successfully.", "Render complete");
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
			this.tileDisplayPanel = new Panel();
			this.currentTileName = new TextBox();
			this.estimatedOutputSizeBox = new TextBox();
			this.textBox1 = new TextBox();
			this.renderErrors = new TextBox();
			this.previewRenderedResultsLinkLabel = new LinkLabel();
			this.viewInBrowserLinkLabel = new LinkLabel();
			this.panel1 = new Panel();
			this.tileDisplayLabel = new TextBox();
			this.renderProgressBar = new ProgressBar();
			this.renderControlButton = new Button();
			this.panel1.SuspendLayout();
			base.SuspendLayout();
			this.tileDisplayPanel.BorderStyle = BorderStyle.FixedSingle;
			this.tileDisplayPanel.Location = new Point(7, 77);
			this.tileDisplayPanel.Name = "tileDisplayPanel";
			this.tileDisplayPanel.Size = new Size(256, 256);
			this.tileDisplayPanel.TabIndex = 31;
			this.tileDisplayPanel.Paint += new PaintEventHandler(this.tileDisplayPanel_Paint);
			this.currentTileName.BackColor = SystemColors.Control;
			this.currentTileName.BorderStyle = BorderStyle.None;
			this.currentTileName.Location = new Point(2, 284);
			this.currentTileName.Name = "currentTileName";
			this.currentTileName.ReadOnly = true;
			this.currentTileName.Size = new Size(261, 13);
			this.currentTileName.TabIndex = 36;
			this.currentTileName.TabStop = false;
			this.estimatedOutputSizeBox.Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
			this.estimatedOutputSizeBox.BackColor = SystemColors.Control;
			this.estimatedOutputSizeBox.BorderStyle = BorderStyle.None;
			this.estimatedOutputSizeBox.Location = new Point(156, 49);
			this.estimatedOutputSizeBox.Name = "estimatedOutputSizeBox";
			this.estimatedOutputSizeBox.ReadOnly = true;
			this.estimatedOutputSizeBox.Size = new Size(566, 13);
			this.estimatedOutputSizeBox.TabIndex = 38;
			this.textBox1.BackColor = SystemColors.Control;
			this.textBox1.BorderStyle = BorderStyle.None;
			this.textBox1.Location = new Point(7, 49);
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new Size(143, 13);
			this.textBox1.TabIndex = 39;
			this.textBox1.TabStop = false;
			this.textBox1.Text = "Status";
			this.renderErrors.Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right);
			this.renderErrors.Location = new Point(269, 77);
			this.renderErrors.Multiline = true;
			this.renderErrors.Name = "renderErrors";
			this.renderErrors.ReadOnly = true;
			this.renderErrors.ScrollBars = ScrollBars.Vertical;
			this.renderErrors.Size = new Size(453, 293);
			this.renderErrors.TabIndex = 40;
			this.previewRenderedResultsLinkLabel.Anchor = (AnchorStyles.Bottom | AnchorStyles.Left);
			this.previewRenderedResultsLinkLabel.AutoSize = true;
			this.previewRenderedResultsLinkLabel.Font = new Font("Microsoft Sans Serif", 11f, FontStyle.Regular, GraphicsUnit.Point, 0);
			this.previewRenderedResultsLinkLabel.Location = new Point(266, 377);
			this.previewRenderedResultsLinkLabel.Name = "previewRenderedResultsLinkLabel";
			this.previewRenderedResultsLinkLabel.Size = new Size(170, 18);
			this.previewRenderedResultsLinkLabel.TabIndex = 41;
			this.previewRenderedResultsLinkLabel.TabStop = true;
			this.previewRenderedResultsLinkLabel.Text = "Preview rendered results";
			this.viewInBrowserLinkLabel.Anchor = (AnchorStyles.Bottom | AnchorStyles.Left);
			this.viewInBrowserLinkLabel.AutoSize = true;
			this.viewInBrowserLinkLabel.Font = new Font("Microsoft Sans Serif", 11f, FontStyle.Regular, GraphicsUnit.Point, 0);
			this.viewInBrowserLinkLabel.Location = new Point(442, 377);
			this.viewInBrowserLinkLabel.Name = "viewInBrowserLinkLabel";
			this.viewInBrowserLinkLabel.Size = new Size(160, 18);
			this.viewInBrowserLinkLabel.TabIndex = 41;
			this.viewInBrowserLinkLabel.TabStop = true;
			this.viewInBrowserLinkLabel.Text = "View results in browser";
			this.viewInBrowserLinkLabel.LinkClicked += new LinkLabelLinkClickedEventHandler(this.viewInBrowserLinkLabel_LinkClicked);
			this.panel1.Controls.Add(this.tileDisplayLabel);
			this.panel1.Controls.Add(this.renderErrors);
			this.panel1.Controls.Add(this.renderProgressBar);
			this.panel1.Controls.Add(this.viewInBrowserLinkLabel);
			this.panel1.Controls.Add(this.previewRenderedResultsLinkLabel);
			this.panel1.Controls.Add(this.estimatedOutputSizeBox);
			this.panel1.Controls.Add(this.tileDisplayPanel);
			this.panel1.Controls.Add(this.textBox1);
			this.panel1.Controls.Add(this.renderControlButton);
			this.panel1.Controls.Add(this.currentTileName);
			this.panel1.Dock = DockStyle.Fill;
			this.panel1.Location = new Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new Size(725, 407);
			this.panel1.TabIndex = 42;
			this.tileDisplayLabel.BackColor = SystemColors.Control;
			this.tileDisplayLabel.BorderStyle = BorderStyle.None;
			this.tileDisplayLabel.Location = new Point(7, 361);
			this.tileDisplayLabel.Multiline = true;
			this.tileDisplayLabel.Name = "tileDisplayLabel";
			this.tileDisplayLabel.ReadOnly = true;
			this.tileDisplayLabel.Size = new Size(256, 66);
			this.tileDisplayLabel.TabIndex = 43;
			this.renderProgressBar.Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
			this.renderProgressBar.Location = new Point(156, 20);
			this.renderProgressBar.Name = "estimateProgressBar";
			this.renderProgressBar.Size = new Size(566, 23);
			this.renderProgressBar.TabIndex = 42;
			this.renderControlButton.Location = new Point(6, 20);
			this.renderControlButton.Name = "estimateControlButton";
			this.renderControlButton.Size = new Size(144, 23);
			this.renderControlButton.TabIndex = 32;
			this.renderControlButton.Text = "Start";
			this.renderControlButton.UseVisualStyleBackColor = true;
			this.renderControlButton.Click += new EventHandler(this.renderControlButton_Click);
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = AutoScaleMode.Font;
			this.BackColor = SystemColors.Control;
			base.Controls.Add(this.panel1);
			base.Name = "RenderProgressPanel2";
			base.Size = new Size(725, 407);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			base.ResumeLayout(false);
		}
	}
}
