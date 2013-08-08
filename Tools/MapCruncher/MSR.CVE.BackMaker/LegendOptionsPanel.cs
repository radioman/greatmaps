using MSR.CVE.BackMaker.ImagePipeline;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
namespace MSR.CVE.BackMaker
{
	public class LegendOptionsPanel : UserControl
	{
		internal class CallbackIgnorinator
		{
			private LegendOptionsPanel lop;
			public CallbackIgnorinator(LegendOptionsPanel lop)
			{
				this.lop = lop;
			}
			public void Callback(AsyncRef asyncRef)
			{
				this.lop.AsyncReadyCallback(this);
			}
		}
		private IContainer components;
		private Label label1;
		private NumericUpDown renderedSizeSpinner;
		private Panel previewPanel;
		private Legend _legend;
		private IDisplayableSource displayableSource;
		private ImageRef previewImage;
		private IFuture previewFuture;
		private InterestList previewInterest;
		private LegendOptionsPanel.CallbackIgnorinator waitingForCI;
		private bool needUpdate;
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
			this.renderedSizeSpinner = new NumericUpDown();
			this.previewPanel = new Panel();
			((ISupportInitialize)this.renderedSizeSpinner).BeginInit();
			base.SuspendLayout();
			this.label1.AutoSize = true;
			this.label1.Location = new Point(7, 18);
			this.label1.Name = "label1";
			this.label1.Size = new Size(75, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Rendered size";
			this.renderedSizeSpinner.Anchor = (AnchorStyles.Top | AnchorStyles.Right);
			NumericUpDown arg_B6_0 = this.renderedSizeSpinner;
			int[] array = new int[4];
			array[0] = 50;
			arg_B6_0.Increment = new decimal(array);
			this.renderedSizeSpinner.Location = new Point(165, 16);
			NumericUpDown arg_ED_0 = this.renderedSizeSpinner;
			int[] array2 = new int[4];
			array2[0] = 1000;
			arg_ED_0.Maximum = new decimal(array2);
			NumericUpDown arg_10A_0 = this.renderedSizeSpinner;
			int[] array3 = new int[4];
			array3[0] = 50;
			arg_10A_0.Minimum = new decimal(array3);
			this.renderedSizeSpinner.Name = "renderedSizeSpinner";
			this.renderedSizeSpinner.Size = new Size(73, 20);
			this.renderedSizeSpinner.TabIndex = 1;
			this.renderedSizeSpinner.TextAlign = HorizontalAlignment.Right;
			NumericUpDown arg_163_0 = this.renderedSizeSpinner;
			int[] array4 = new int[4];
			array4[0] = 50;
			arg_163_0.Value = new decimal(array4);
			this.previewPanel.Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right);
			this.previewPanel.Location = new Point(10, 50);
			this.previewPanel.Name = "previewPanel";
			this.previewPanel.Size = new Size(228, 235);
			this.previewPanel.TabIndex = 2;
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = AutoScaleMode.Font;
			base.Controls.Add(this.previewPanel);
			base.Controls.Add(this.renderedSizeSpinner);
			base.Controls.Add(this.label1);
			base.Name = "LegendOptionsPanel";
			base.Size = new Size(250, 296);
			((ISupportInitialize)this.renderedSizeSpinner).EndInit();
			base.ResumeLayout(false);
			base.PerformLayout();
		}
		public LegendOptionsPanel()
		{
			this.InitializeComponent();
			this.renderedSizeSpinner.ValueChanged += new EventHandler(this.renderedSizeSpinner_ValueChanged);
			this.previewPanel.Paint += new PaintEventHandler(this.previewPanel_Paint);
			this.renderedSizeSpinner.Minimum = Legend.renderedSizeRange.min;
			this.renderedSizeSpinner.Maximum = Legend.renderedSizeRange.max;
		}
		public void Configure(Legend legend, IDisplayableSource displayableSource)
		{
			Monitor.Enter(this);
			try
			{
				if (this.previewInterest != null)
				{
					this.previewInterest.Dispose();
					this.previewInterest = null;
					this.waitingForCI = null;
				}
			}
			finally
			{
				Monitor.Exit(this);
			}
			if (this._legend == legend)
			{
				return;
			}
			if (this._legend != null)
			{
				this._legend.dirtyEvent.Remove(new DirtyListener(this.LegendChanged));
			}
			this._legend = legend;
			this.displayableSource = displayableSource;
			if (this._legend != null)
			{
				this._legend.dirtyEvent.Add(new DirtyListener(this.LegendChanged));
				this.LegendChanged();
			}
			base.Enabled = (this._legend != null);
			this.UpdatePreviewImage(null);
			this.UpdatePreviewPanel();
		}
		private void UpdatePreviewImage(ImageRef imageRef)
		{
			Monitor.Enter(this);
			try
			{
				if (this.previewImage != null)
				{
					this.previewImage.Dispose();
					this.previewImage = null;
				}
				if (imageRef != null)
				{
					this.previewImage = (ImageRef)imageRef.Duplicate("LegendOptionsPanel.UpdatePreviewImage");
				}
			}
			finally
			{
				Monitor.Exit(this);
			}
			base.Invalidate();
		}
		private void LegendChanged()
		{
			this.renderedSizeSpinner.Value = this._legend.renderedSize;
			this.UpdatePreviewPanel();
		}
		private void renderedSizeSpinner_ValueChanged(object sender, EventArgs e)
		{
			this._legend.renderedSize = (int)this.renderedSizeSpinner.Value;
		}
		private void UpdatePreviewPanel()
		{
			this.needUpdate = true;
			base.Invalidate();
		}
		private void HandleUpdate()
		{
			if (!this.needUpdate)
			{
				return;
			}
			this.needUpdate = false;
			Monitor.Enter(this);
			try
			{
				if (this.previewInterest != null)
				{
					this.previewInterest.Dispose();
					this.previewInterest = null;
					this.waitingForCI = null;
				}
				if (this._legend != null)
				{
					try
					{
						IFuture renderedLegendFuture = this._legend.GetRenderedLegendFuture(this.displayableSource, (FutureFeatures)5);
						if (this.previewFuture != renderedLegendFuture)
						{
							this.previewFuture = renderedLegendFuture;
							AsyncRef asyncRef = (AsyncRef)renderedLegendFuture.Realize("LegendOptionsPanel.UpdatePreviewPanel");
							if (asyncRef.present == null)
							{
								this.waitingForCI = new LegendOptionsPanel.CallbackIgnorinator(this);
								asyncRef.AddCallback(new AsyncRecord.CompleteCallback(this.waitingForCI.Callback));
								asyncRef.SetInterest(524296);
								this.previewInterest = new InterestList();
								this.previewInterest.Add(asyncRef);
								this.previewInterest.Activate();
								this.UpdatePreviewImage(null);
							}
							else
							{
								if (asyncRef.present is ImageRef)
								{
									this.UpdatePreviewImage((ImageRef)asyncRef.present);
								}
							}
						}
					}
					catch (Legend.RenderFailedException)
					{
					}
				}
			}
			finally
			{
				Monitor.Exit(this);
			}
		}
		internal void AsyncReadyCallback(LegendOptionsPanel.CallbackIgnorinator ci)
		{
			if (ci == this.waitingForCI)
			{
				this.UpdatePreviewPanel();
			}
		}
		private void previewPanel_Paint(object sender, PaintEventArgs e)
		{
			this.HandleUpdate();
			ImageRef imageRef = null;
			Monitor.Enter(this);
			try
			{
				if (this.previewImage != null)
				{
					imageRef = (ImageRef)this.previewImage.Duplicate("LegendOptionsPanel.previewPanel_Paint");
				}
			}
			finally
			{
				Monitor.Exit(this);
			}
			if (imageRef != null)
			{
				try
				{
					GDIBigLockedImage image;
					Monitor.Enter(image = imageRef.image);
					try
					{
						Image image2 = imageRef.image.IPromiseIAmHoldingGDISLockSoPleaseGiveMeTheImage();
						e.Graphics.DrawImage(image2, new Rectangle(new Point(0, 0), this.previewPanel.Size), new Rectangle(new Point(0, 0), this.previewPanel.Size), GraphicsUnit.Pixel);
					}
					finally
					{
						Monitor.Exit(image);
					}
				}
				catch (Exception)
				{
					D.Say(0, "Absorbing that disturbing bug wherein the mostRecentTile image is corrupt.");
				}
				imageRef.Dispose();
				return;
			}
			e.Graphics.DrawRectangle(new Pen(Color.Black), 0, 0, this.previewPanel.Size.Width - 1, this.previewPanel.Height - 1);
		}
	}
}
