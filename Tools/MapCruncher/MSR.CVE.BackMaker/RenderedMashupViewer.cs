using MSR.CVE.BackMaker.ImagePipeline;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Windows.Forms;
namespace MSR.CVE.BackMaker
{
	public class RenderedMashupViewer : Form
	{
		private MapPosition mapPos;
		private CachePackage cachePackage;
		private PrintDocument printDoc;
		private IContainer components;
		private MenuStrip menuStrip1;
		private ToolStripMenuItem vEBackgroundToolStripMenuItem;
		private ToolStripMenuItem VEroadView;
		private ToolStripMenuItem VEaerialView;
		private ToolStripMenuItem VEhybridView;
		private ToolStripMenuItem mashupLayersMenuItem;
		private ToolStripMenuItem addLayerToolStripMenuItem;
		private ToolStripSeparator toolStripSeparator1;
		private ViewerControl viewer;
		private ToolStripMenuItem fileToolStripMenuItem;
		private ToolStripMenuItem printToolStripMenuItem;
		private ToolStripMenuItem pageSetupToolStripMenuItem;
		private ToolStripMenuItem printPreviewToolStripMenuItem;
		public RenderedMashupViewer(CachePackage cachePackage, ToolStripMenuItem dmsMenuItem)
		{
			this.InitializeComponent();
			this.cachePackage = cachePackage;
			this.mapPos = new MapPosition(this.viewer);
			this.viewer.Initialize(new MapPositionDelegate(this.GetMapPos), "Map Location");
			this.viewer.ShowDMS = new MapDrawingOption(this.viewer, dmsMenuItem, false);
			this.SetVEMapStyle(VirtualEarthWebDownloader.RoadStyle);
			this.mapPos.setPosition(this.viewer.GetCoordinateSystem().GetDefaultView());
			this.printDoc = new PrintDocument();
			this.printDoc.PrintPage += new PrintPageEventHandler(this.PrintPage);
		}
		private MapPosition GetMapPos()
		{
			return this.mapPos;
		}
		private void SetVEMapStyle(string s)
		{
			if (!VirtualEarthWebDownloader.StyleIsValid(s))
			{
				return;
			}
			this.VEroadView.Checked = (s == VirtualEarthWebDownloader.RoadStyle);
			this.VEaerialView.Checked = (s == VirtualEarthWebDownloader.AerialStyle);
			this.VEhybridView.Checked = (s == VirtualEarthWebDownloader.HybridStyle);
			this.viewer.SetBaseLayer(new VETileSource(this.cachePackage, s));
		}
		private void VEroadView_Click(object sender, EventArgs e)
		{
			this.SetVEMapStyle(VirtualEarthWebDownloader.RoadStyle);
		}
		private void VEaerialView_Click(object sender, EventArgs e)
		{
			this.SetVEMapStyle(VirtualEarthWebDownloader.AerialStyle);
		}
		private void VEhybridView_Click(object sender, EventArgs e)
		{
			this.SetVEMapStyle(VirtualEarthWebDownloader.HybridStyle);
		}
		private void addLayerToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.addLayers(RenderedLayerSelector.GetLayerSelector(this.viewer, this.cachePackage));
		}
		private void addLayers(RenderedLayerDisplayInfo displayInfo)
		{
			if (displayInfo != null)
			{
				foreach (ToolStripMenuItem current in displayInfo.tsmiList)
				{
					this.mashupLayersMenuItem.DropDownItems.Add(current);
				}
				this.mapPos.setPosition(displayInfo.defaultView);
			}
		}
		internal void AddLayersFromUri(Uri uri)
		{
			this.addLayers(RenderedLayerSelector.GetLayerSelector(this.viewer, this.cachePackage, uri));
		}
		private void printToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (new PrintDialog
			{
				Document = this.printDoc
			}.ShowDialog() == DialogResult.OK)
			{
				this.printDoc.Print();
			}
		}
		private void pageSetupToolStripMenuItem_Click(object sender, EventArgs e)
		{
			PageSetupDialog pageSetupDialog = new PageSetupDialog();
			pageSetupDialog.Document = this.printDoc;
			pageSetupDialog.AllowOrientation = true;
			pageSetupDialog.AllowMargins = true;
			pageSetupDialog.AllowPaper = true;
			this.DebugPrintSettings();
			pageSetupDialog.ShowDialog();
			this.DebugPrintSettings();
		}
		private void DebugPrintSettings()
		{
			D.Say(0, string.Format("Printer {0} Paper {1} Width {2} Landscape {3} Color {4}", new object[]
			{
				this.printDoc.PrinterSettings.PrinterName,
				this.printDoc.DefaultPageSettings.PaperSize,
				this.printDoc.DefaultPageSettings.PaperSize.Width,
				this.printDoc.DefaultPageSettings.Landscape,
				this.printDoc.DefaultPageSettings.Color
			}));
		}
		private void PrintPage(object sender, PrintPageEventArgs e)
		{
			e.Graphics.TranslateTransform((float)e.MarginBounds.X, (float)e.MarginBounds.Y);
			Rectangle rectangle = new Rectangle(0, 0, e.MarginBounds.Width, e.MarginBounds.Height);
			e.Graphics.SetClip(rectangle);
			int num = 4;
			float num2 = (float)(1 << num);
			e.Graphics.ScaleTransform(1f / num2, 1f / num2);
			rectangle.Width = (int)((float)rectangle.Width * num2);
			rectangle.Height = (int)((float)rectangle.Height * num2);
			GraphicsContainer container = e.Graphics.BeginContainer();
			PaintSpecification e2 = new PaintSpecification(e.Graphics, rectangle, rectangle.Size, true);
			this.viewer.PaintPrintWindow(e2, num);
			e.Graphics.EndContainer(container);
		}
		private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
		{
			new PrintPreviewDialog
			{
				Document = this.printDoc
			}.ShowDialog();
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
			ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(RenderedMashupViewer));
			this.menuStrip1 = new MenuStrip();
			this.fileToolStripMenuItem = new ToolStripMenuItem();
			this.pageSetupToolStripMenuItem = new ToolStripMenuItem();
			this.printPreviewToolStripMenuItem = new ToolStripMenuItem();
			this.printToolStripMenuItem = new ToolStripMenuItem();
			this.vEBackgroundToolStripMenuItem = new ToolStripMenuItem();
			this.VEroadView = new ToolStripMenuItem();
			this.VEaerialView = new ToolStripMenuItem();
			this.VEhybridView = new ToolStripMenuItem();
			this.mashupLayersMenuItem = new ToolStripMenuItem();
			this.addLayerToolStripMenuItem = new ToolStripMenuItem();
			this.toolStripSeparator1 = new ToolStripSeparator();
			this.viewer = new ViewerControl();
			this.menuStrip1.SuspendLayout();
			base.SuspendLayout();
			this.menuStrip1.Items.AddRange(new ToolStripItem[]
			{
				this.fileToolStripMenuItem,
				this.vEBackgroundToolStripMenuItem,
				this.mashupLayersMenuItem
			});
			this.menuStrip1.Location = new Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new Size(792, 24);
			this.menuStrip1.TabIndex = 0;
			this.menuStrip1.Text = "menuStrip1";
			this.fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[]
			{
				this.pageSetupToolStripMenuItem,
				this.printPreviewToolStripMenuItem,
				this.printToolStripMenuItem
			});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new Size(35, 20);
			this.fileToolStripMenuItem.Text = "File";
			this.fileToolStripMenuItem.Visible = false;
			this.pageSetupToolStripMenuItem.Name = "pageSetupToolStripMenuItem";
			this.pageSetupToolStripMenuItem.Size = new Size(160, 22);
			this.pageSetupToolStripMenuItem.Text = "Page Setup...";
			this.pageSetupToolStripMenuItem.Click += new EventHandler(this.pageSetupToolStripMenuItem_Click);
			this.printPreviewToolStripMenuItem.Name = "printPreviewToolStripMenuItem";
			this.printPreviewToolStripMenuItem.Size = new Size(160, 22);
			this.printPreviewToolStripMenuItem.Text = "Print Preview...";
			this.printPreviewToolStripMenuItem.Click += new EventHandler(this.printPreviewToolStripMenuItem_Click);
			this.printToolStripMenuItem.Name = "printToolStripMenuItem";
			this.printToolStripMenuItem.Size = new Size(160, 22);
			this.printToolStripMenuItem.Text = "Print...";
			this.printToolStripMenuItem.Click += new EventHandler(this.printToolStripMenuItem_Click);
			this.vEBackgroundToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[]
			{
				this.VEroadView,
				this.VEaerialView,
				this.VEhybridView
			});
			this.vEBackgroundToolStripMenuItem.Name = "vEBackgroundToolStripMenuItem";
			this.vEBackgroundToolStripMenuItem.Size = new Size(90, 20);
			this.vEBackgroundToolStripMenuItem.Text = "VE Background";
			this.VEroadView.Name = "VEroadView";
			this.VEroadView.Size = new Size(148, 22);
			this.VEroadView.Text = "Roads";
			this.VEroadView.Click += new EventHandler(this.VEroadView_Click);
			this.VEaerialView.Name = "VEaerialView";
			this.VEaerialView.Size = new Size(148, 22);
			this.VEaerialView.Text = "Aerial Photos";
			this.VEaerialView.Click += new EventHandler(this.VEaerialView_Click);
			this.VEhybridView.Name = "VEhybridView";
			this.VEhybridView.Size = new Size(148, 22);
			this.VEhybridView.Text = "Hybrid";
			this.VEhybridView.Click += new EventHandler(this.VEhybridView_Click);
			this.mashupLayersMenuItem.DropDownItems.AddRange(new ToolStripItem[]
			{
				this.addLayerToolStripMenuItem,
				this.toolStripSeparator1
			});
			this.mashupLayersMenuItem.Name = "mashupLayersMenuItem";
			this.mashupLayersMenuItem.Size = new Size(91, 20);
			this.mashupLayersMenuItem.Text = "Mashup Layers";
			this.addLayerToolStripMenuItem.Name = "addLayerToolStripMenuItem";
			this.addLayerToolStripMenuItem.Size = new Size(146, 22);
			this.addLayerToolStripMenuItem.Text = "Add Layer...";
			this.addLayerToolStripMenuItem.Click += new EventHandler(this.addLayerToolStripMenuItem_Click);
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new Size(143, 6);
			this.viewer.Dock = DockStyle.Fill;
			this.viewer.Location = new Point(0, 24);
			this.viewer.Name = "viewer";
			this.viewer.Size = new Size(792, 542);
			this.viewer.TabIndex = 1;
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			//base.AutoScaleMode = AutoScaleMode.Font;
			base.ClientSize = new Size(792, 566);
			base.Controls.Add(this.viewer);
			base.Controls.Add(this.menuStrip1);
			//base.Icon = (Icon)componentResourceManager.GetObject("$this.Icon");
			base.MainMenuStrip = this.menuStrip1;
			base.Name = "RenderedMashupViewer";
			this.Text = "Mashup Viewer";
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			base.ResumeLayout(false);
			base.PerformLayout();
		}
	}
}
