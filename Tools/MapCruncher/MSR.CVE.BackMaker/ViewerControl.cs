using MSR.CVE.BackMaker.ImagePipeline;
using MSR.CVE.BackMaker.MCDebug;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;
namespace MSR.CVE.BackMaker
{
	public class ViewerControl : UserControl, SVDisplayParams, PinDisplayIfc, PositionUpdateIfc, InvalidatableViewIfc, LatLonEditIfc, ViewerControlIfc, TransparencyIfc, SnapViewDisplayIfc
	{
		public interface MouseAction
		{
			void Dragged(Point diff);
			Cursor GetCursor(bool dragging);
			void OnPopup(ContextMenu menu);
		}
		public class NoAction : ViewerControl.MouseAction
		{
			public void Dragged(Point diff)
			{
			}
			public void OnPopup(ContextMenu menu)
			{
			}
			public Cursor GetCursor(bool dragging)
			{
				return Cursors.No;
			}
		}
		public class DragImageAction : ViewerControl.MouseAction
		{
			private ViewerControl sourceViewer;
			public DragImageAction(ViewerControl sourceViewer)
			{
				this.sourceViewer = sourceViewer;
			}
			public void Dragged(Point diff)
			{
				this.sourceViewer.DragOnImage(diff);
			}
			public Cursor GetCursor(bool dragging)
			{
				if (!dragging)
				{
					return Cursors.Hand;
				}
				return Cursors.Hand;
			}
			public void OnPopup(ContextMenu menu)
			{
			}
		}
		private interface TilePaintClosure : IDisposable
		{
			void PaintTile(Graphics g, Rectangle paintLocation);
		}
		private class ImagePainter : ViewerControl.TilePaintClosure, IDisposable
		{
			private ImageRef imageRef;
			private Region clipRegion;
			public ImagePainter(ImageRef imageRef, Region clipRegion)
			{
				this.imageRef = imageRef;
				this.clipRegion = clipRegion;
			}
			public void PaintTile(Graphics g, Rectangle paintLocation)
			{
				if (this.clipRegion != null)
				{
					g.Clip = this.clipRegion;
				}
				GDIBigLockedImage image;
				Monitor.Enter(image = this.imageRef.image);
				try
				{
					g.DrawImage(this.imageRef.image.IPromiseIAmHoldingGDISLockSoPleaseGiveMeTheImage(), paintLocation, new Rectangle(new Point(0, 0), this.imageRef.image.Size), GraphicsUnit.Pixel);
				}
				finally
				{
					Monitor.Exit(image);
				}
			}
			public void Dispose()
			{
				this.imageRef.Dispose();
			}
		}
		private class NullPainter : ViewerControl.TilePaintClosure, IDisposable
		{
			public void PaintTile(Graphics g, Rectangle paintLocation)
			{
			}
			public void Dispose()
			{
			}
		}
		private class MessagePainter : ViewerControl.TilePaintClosure, IDisposable
		{
			private int offsetPixels;
			private string message;
			private bool fillBG;
			public MessagePainter(int offsetPixels, string message, bool fillBG)
			{
				this.offsetPixels = offsetPixels;
				this.message = message;
				this.fillBG = fillBG;
			}
			public void PaintTile(Graphics g, Rectangle paintLocation)
			{
				Brush brush = new SolidBrush(Color.LightGray);
				if (this.fillBG)
				{
					g.FillRectangle(brush, paintLocation);
				}
				for (float num = 0.2f; num < 1f; num += 0.6f)
				{
					Font font = new Font("Arial", 8f);
					PointF pointF = new PointF((float)paintLocation.Left + (float)paintLocation.Width * 0.02f + (float)this.offsetPixels, (float)paintLocation.Top + (float)paintLocation.Height * num);
					SizeF size = g.MeasureString(this.message, font);
					g.FillEllipse(new SolidBrush(Color.Wheat), new RectangleF(pointF, size));
					g.DrawString(this.message, font, new SolidBrush(Color.Crimson), pointF);
				}
			}
			public void Dispose()
			{
			}
		}
		private class TileNamePainter : ViewerControl.TilePaintClosure, IDisposable
		{
			private string tileName;
			public TileNamePainter(string tileName)
			{
				this.tileName = tileName;
			}
			public void PaintTile(Graphics g, Rectangle paintLocation)
			{
				Font font = new Font("Helvetica", 10f);
				SizeF sizeF = g.MeasureString(this.tileName, font);
				PointF point = new Point(paintLocation.X + 20, paintLocation.Y + 8);
				float num = 5f;
				g.CompositingMode = CompositingMode.SourceOver;
				Brush brush = new SolidBrush(Color.FromArgb(40, 0, 0, 0));
				g.FillRectangle(brush, new RectangleF(new PointF(point.X - num, point.Y - num), new SizeF(sizeF.Width + 2f * num, sizeF.Height + 2f * num)));
				g.DrawString(this.tileName, font, new SolidBrush(Color.Crimson), point);
			}
			public void Dispose()
			{
			}
		}
		private class TileBoundaryPainter : ViewerControl.TilePaintClosure, IDisposable
		{
			public void PaintTile(Graphics g, Rectangle paintLocation)
			{
				g.DrawRectangle(new Pen(Color.Crimson), paintLocation);
			}
			public void Dispose()
			{
			}
		}
		private class PaintKit
		{
			public Rectangle paintLocation;
			public List<ViewerControl.TilePaintClosure> meatyParts = new List<ViewerControl.TilePaintClosure>();
			public List<ViewerControl.TilePaintClosure> annotations = new List<ViewerControl.TilePaintClosure>();
			public PaintKit(Rectangle paintLocation)
			{
				this.paintLocation = paintLocation;
			}
		}
		private class AsyncNotifier
		{
			private ViewerControl viewerControl;
			private int generation;
			public AsyncNotifier(ViewerControl viewerControl)
			{
				this.viewerControl = viewerControl;
				this.generation = viewerControl.asyncRequestGeneration;
			}
			public void AsyncRecordComplete(AsyncRef asyncRef)
			{
				if (this.viewerControl.asyncRequestGeneration == this.generation)
				{
					this.viewerControl.InvalidateView();
				}
			}
		}
		private const int ecRadius = 6;
		private const int invertErrorRadius = 20;
		private DisplayableSourceCache baseLayer;
		private List<DisplayableSourceCache> alphaLayers = new List<DisplayableSourceCache>();
		private List<PositionAssociationView> pinList;
		private MapPositionDelegate center;
		private InterestList activeTiles;
		private UserRegionViewController userRegionViewController;
		private LatentRegionHolder latentRegionHolder;
		private Point drag_origin;
		private bool is_dragging;
		private ViewerControl.MouseAction imminentAction = new ViewerControl.NoAction();
		public MapDrawingOption ShowCrosshairs;
		public MapDrawingOption ShowTileBoundaries;
		public MapDrawingOption ShowTileNames;
		public MapDrawingOption ShowPushPins;
		public MapDrawingOption ShowSourceCrop;
		private Font pinFont;
		private Brush fillBrush;
		private Brush textBrush;
		private Pen outlinePen;
		private Brush errorContribBrush;
		private Pen errorContribPen;
		private Brush errorOutlierBrush;
		private Pen errorOutlierPen;
		private int tilesRequired;
		private int tilesAvailable;
		private int asyncRequestGeneration;
		private SnapViewStoreIfc snapViewStore;
		private IContainer components;
		private LLZBox llzBox;
		private Button zoomOutButton;
		private Button zoomInButton;
		private TextBox creditsTextBox;
		private Button zenButton;
		private ProgressBar displayProgressBar;

        //[CompilerGenerated]
        //private static Comparison<PositionAssociationView> <>9__CachedAnonymousMethodDelegate4;

		public MapDrawingOption ShowDMS
		{
			set
			{
				this.llzBox.ShowDMS = value;
			}
		}
		public ViewerControl()
		{
			this.InitializeComponent();

			this.center = new MapPositionDelegate(new NoMapPosition().NoMapPositionDelegate);
			this.Dock = DockStyle.Fill;
			base.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
			this.ContextMenu = new ContextMenu();
			this.ContextMenu.Popup += new EventHandler(this.HandlePopup);
			base.Layout += new LayoutEventHandler(this.ViewerControl_Layout);
			this.zenButton.Size = new Size(0, 0);
			this.zenButton.KeyDown += new KeyEventHandler(this.zenButton_KeyDown);
			this.zenButton.KeyUp += new KeyEventHandler(this.zenButton_KeyUp);
			this.InitAppearance();
		}
		public void configureLLZBoxEditable()
		{
			this.llzBox.configureEditable(this);
		}
		public void latEdited(double newLat)
		{
			LatLon latlon = new LatLon(newLat, this.center().llz.lon);
			latlon.CheckValid(this.GetCoordinateSystem());
			this.center().setPosition(new LatLonZoom(latlon, this.center().llz.zoom));
			this.center().ForceInteractiveUpdate();
		}
		public void lonEdited(double newLon)
		{
			LatLon latlon = new LatLon(this.center().llz.lat, newLon);
			latlon.CheckValid(this.GetCoordinateSystem());
			this.center().setPosition(new LatLonZoom(latlon, this.center().llz.zoom));
			this.center().ForceInteractiveUpdate();
		}
		private void zenButton_KeyDown(object sender, KeyEventArgs e)
		{
			bool handled = false;
			if ((e.KeyData & Keys.Control) == Keys.Control)
			{
				handled = true;
				if (e.KeyCode == Keys.Up)
				{
					this.DragOnImage(new Point(0, -1));
				}
				else
				{
					if (e.KeyCode == Keys.Down)
					{
						this.DragOnImage(new Point(0, 1));
					}
					else
					{
						if (e.KeyCode == Keys.Left)
						{
							this.DragOnImage(new Point(-1, 0));
						}
						else
						{
							if (e.KeyCode == Keys.Right)
							{
								this.DragOnImage(new Point(1, 0));
							}
							else
							{
								handled = false;
							}
						}
					}
				}
			}
			e.Handled = handled;
		}
		private void zenButton_KeyUp(object sender, KeyEventArgs e)
		{
			if (BuildConfig.theConfig.enableSnapFeatures)
			{
				if (e.KeyCode == Keys.F5)
				{
					if ((e.KeyData & Keys.Shift) == Keys.Shift)
					{
						this.RecordSnapView();
					}
					else
					{
						this.RestoreSnapView();
					}
					e.Handled = true;
					return;
				}
				if (e.KeyCode == Keys.F6)
				{
					if ((e.KeyData & Keys.Shift) == Keys.Shift)
					{
						this.RecordSnapZoom();
					}
					else
					{
						this.RestoreSnapZoom();
					}
					e.Handled = true;
				}
			}
		}
		public void SetSnapViewStore(SnapViewStoreIfc snapViewStore)
		{
			this.snapViewStore = snapViewStore;
		}
		public void RecordSnapView()
		{
			if (this.snapViewStore != null)
			{
				this.snapViewStore.Record(this.center().llz);
			}
		}
		public void RestoreSnapView()
		{
			if (this.snapViewStore != null)
			{
				LatLonZoom latLonZoom = this.snapViewStore.Restore();
				if (latLonZoom != default(LatLonZoom))
				{
					this.center().setPosition(latLonZoom);
					this.center().ForceInteractiveUpdate();
				}
			}
		}
		public void RecordSnapZoom()
		{
			if (this.snapViewStore != null)
			{
				this.snapViewStore.RecordZoom(this.center().llz.zoom);
			}
		}
		public void RestoreSnapZoom()
		{
			if (this.snapViewStore != null)
			{
				int num = this.snapViewStore.RestoreZoom();
				if (num != 0)
				{
					this.center().setPosition(new LatLonZoom(this.center().llz.latlon, num));
					this.center().ForceInteractiveUpdate();
				}
			}
		}
		public void Initialize(MapPositionDelegate mpd, string LLZBoxName)
		{
			this.center = mpd;
			this.llzBox.setName(LLZBoxName);
		}
		public void SetLLZBoxLabelStyle(LLZBox.LabelStyle labelStyle)
		{
			this.llzBox.SetLabelStyle(labelStyle);
		}
		private void InitAppearance()
		{
			this.pinFont = new Font(new FontFamily("Arial"), 10f, FontStyle.Bold);
			this.fillBrush = new SolidBrush(Color.White);
			this.textBrush = new SolidBrush(Color.Red);
			this.outlinePen = new Pen(this.textBrush, 2f);
			this.errorContribBrush = new SolidBrush(Color.Green);
			this.errorContribPen = new Pen(Color.DarkGreen, 2f);
			this.errorOutlierBrush = new SolidBrush(Color.Blue);
			this.errorOutlierPen = new Pen(Color.DarkBlue, 2f);
		}
		private void ViewerControl_Layout(object sender, LayoutEventArgs e)
		{
			this.MakeCreditsVisible();
		}
		private void MakeCreditsVisible()
		{
			this.creditsTextBox.SelectionStart = this.creditsTextBox.Text.Length;
			this.creditsTextBox.SelectionLength = 0;
			this.creditsTextBox.ScrollToCaret();
		}
		public void ClearLayers()
		{
			this.baseLayer = null;
			this.latentRegionHolder = null;
			this.alphaLayers = new List<DisplayableSourceCache>();
			this.SetCreditString(null);
		}
		public void SetBaseLayer(IDisplayableSource tileSource)
		{
			this.baseLayer = new DisplayableSourceCache(tileSource);
			this.SetCreditString(tileSource.GetRendererCredit());
			this.latentRegionHolder = null;
			this.userRegionViewController = null;
			this.InvalidateView();
		}
		public void SetLatentRegionHolder(LatentRegionHolder latentRegionHolder)
		{
			this.latentRegionHolder = latentRegionHolder;
			this.userRegionViewController = null;
			this.InvalidateView();
		}
		public void SetCreditString(string credit)
		{
			if (credit == null)
			{
				this.creditsTextBox.Visible = false;
				return;
			}
			this.creditsTextBox.Visible = true;
			this.creditsTextBox.Text = credit;
			this.MakeCreditsVisible();
		}
		public MapRectangle GetBounds()
		{
			return CoordinateSystemUtilities.GetBounds(this.baseLayer.GetDefaultCoordinateSystem(), this.center().llz, base.Size);
		}
		public CoordinateSystemIfc GetCoordinateSystem()
		{
			return this.baseLayer.GetDefaultCoordinateSystem();
		}
		public void AddAlphaLayer(IDisplayableSource tileSource)
		{
			this.alphaLayers.Add(new DisplayableSourceCache(tileSource));
			this.InvalidateView();
		}
		public void RemoveAlphaLayer(IDisplayableSource tileSource)
		{
			int index = this.alphaLayers.FindIndex((DisplayableSourceCache dsc0) => dsc0.BackingStoreIs(tileSource));
			this.alphaLayers.RemoveAt(index);
			this.InvalidateView();
		}
		public void setPinList(List<PositionAssociationView> newList)
		{
			this.pinList = newList;
			base.Invalidate();
		}
		private void zoomOutButton_Click(object sender, EventArgs e)
		{
			this.zoom(-1);
		}
		private void zoomInButton_Click(object sender, EventArgs e)
		{
			this.zoom(1);
		}
		public void zoom(int zoomFactor)
		{
			if (this.baseLayer != null)
			{
				this.center().setPosition(CoordinateSystemUtilities.GetZoomedView(this.GetCoordinateSystem(), this.center().llz, zoomFactor));
			}
		}
		private ViewerControl.MouseAction ImminentAction(MouseEventArgs e)
		{
			ViewerControl.MouseAction mouseAction = null;
			if (this.userRegionViewController != null)
			{
				mouseAction = this.userRegionViewController.ImminentAction(e);
			}
			if (mouseAction == null)
			{
				mouseAction = new ViewerControl.DragImageAction(this);
			}
			return mouseAction;
		}
		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			base.Invalidate();
		}
		protected override void OnMouseClick(MouseEventArgs e)
		{
			this.zenButton.Focus();
			base.OnMouseClick(e);
		}
		protected override void OnMouseWheel(MouseEventArgs e)
		{
			D.Say(3, string.Format("Zooming -- mousedelta={0}", e.Delta));
			this.zoom(e.Delta / 120);
			base.OnMouseWheel(e);
		}
		protected void HandlePopup(object sender, EventArgs e)
		{
			this.ContextMenu.MenuItems.Clear();
			this.imminentAction.OnPopup(this.ContextMenu);
		}
		protected override void OnMouseDown(MouseEventArgs e)
		{
			this.is_dragging = true;
			this.drag_origin = new Point(e.X, e.Y);
			this.imminentAction = this.ImminentAction(e);
			Cursor.Current = this.imminentAction.GetCursor(true);
			base.OnMouseDown(e);
		}
		protected override void OnMouseUp(MouseEventArgs e)
		{
			this.is_dragging = false;
			this.imminentAction = this.ImminentAction(e);
			Cursor.Current = this.imminentAction.GetCursor(false);
			base.OnMouseUp(e);
		}
		protected override void OnMouseLeave(EventArgs e)
		{
			this.is_dragging = false;
			this.imminentAction = new ViewerControl.NoAction();
			base.OnMouseLeave(e);
		}
		private void DragOnImage(Point diff)
		{
			this.center().setPosition(this.GetCoordinateSystem().GetTranslationInLatLon(this.center().llz, diff));
			this.center().ForceInteractiveUpdate();
		}
		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (this.baseLayer != null)
			{
				if (this.is_dragging)
				{
					Point diff = new Point(e.X - this.drag_origin.X, e.Y - this.drag_origin.Y);
					this.imminentAction.Dragged(diff);
					base.Invalidate();
					this.drag_origin = new Point(e.X, e.Y);
				}
				else
				{
					this.imminentAction = this.ImminentAction(e);
					Cursor.Current = this.imminentAction.GetCursor(false);
				}
			}
			base.OnMouseMove(e);
		}
		public Point ScreenCenter()
		{
			return new Point(base.Size.Width / 2, base.Size.Height / 2);
		}
		public LatLonZoom MapCenter()
		{
			return this.center().llz;
		}
		protected override void OnMouseDoubleClick(MouseEventArgs e)
		{
			if (this.baseLayer != null)
			{
				Point point = this.ScreenCenter();
				Point offsetInPixels = new Point(point.X - e.Location.X, point.Y - e.Location.Y);
				this.center().setPosition(CoordinateSystemUtilities.GetZoomedView(this.GetCoordinateSystem(), this.GetCoordinateSystem().GetTranslationInLatLon(this.center().llz, offsetInPixels), 1));
			}
			base.OnMouseDoubleClick(e);
		}
		protected override void OnKeyDown(KeyEventArgs e)
		{
			bool handled = false;
			if ((e.KeyData & Keys.Control) == Keys.Control)
			{
				handled = true;
				if (e.KeyCode == Keys.Up)
				{
					this.DragOnImage(new Point(0, -1));
				}
				else
				{
					if (e.KeyCode == Keys.Down)
					{
						this.DragOnImage(new Point(0, 1));
					}
					else
					{
						if (e.KeyCode == Keys.Left)
						{
							this.DragOnImage(new Point(-1, 0));
						}
						else
						{
							if (e.KeyCode == Keys.Right)
							{
								this.DragOnImage(new Point(1, 0));
							}
							else
							{
								handled = false;
							}
						}
					}
				}
			}
			e.Handled = handled;
			base.OnKeyDown(e);
		}
		public ImageRef MessageImage(string message, Size tileSize)
		{
			GDIBigLockedImage gDIBigLockedImage = new GDIBigLockedImage(tileSize, "ViewerControl-MessageImage");
			Graphics graphics = gDIBigLockedImage.IPromiseIAmHoldingGDISLockSoPleaseGiveMeTheGraphics();
			Brush brush = new SolidBrush(Color.LightGray);
			graphics.FillRectangle(brush, 0, 0, tileSize.Width, tileSize.Height);
			graphics.DrawString(message, new Font("Arial", 10f), new SolidBrush(Color.Crimson), new PointF((float)tileSize.Width * 0.02f, (float)tileSize.Height * 0.2f));
			graphics.DrawString(message, new Font("Arial", 10f), new SolidBrush(Color.Crimson), new PointF((float)tileSize.Width * 0.02f, (float)tileSize.Height * 0.8f));
			return new ImageRef(new ImageRefCounted(gDIBigLockedImage));
		}
		private PointF MapPositionToPoint(LatLon pos)
		{
			Point translationInPixels = this.GetCoordinateSystem().GetTranslationInPixels(this.center().llz, pos);
			PointF result = new PointF((float)(base.Width / 2 + translationInPixels.X), (float)(base.Height / 2 + translationInPixels.Y));
			return result;
		}
		private void DrawMarker(PositionAssociationView pav, PaintSpecification e)
		{
			this.DrawErrorMarkers(pav, e);
			e.Graphics.CompositingMode = CompositingMode.SourceOver;
			string text = pav.pinId.ToString();
			this.outlinePen.MiterLimit = 2f;
			SizeF size = new SizeF(3f, 3f);
			PointF pointF = this.MapPositionToPoint(pav.position.pinPosition.latlon);
			if (!RectangleF.Inflate(e.ClipRectangle, 100f, 100f).Contains(pointF))
			{
				return;
			}
			SizeF sizeF = e.Graphics.MeasureString(text, this.pinFont);
			double num = 24.0;
			double num2 = 3.0;
			int num3 = 3;
			RectangleF layoutRectangle = new RectangleF(pointF.X - sizeF.Width / 2f, (float)((double)(pointF.Y - sizeF.Height / 2f) - num), sizeF.Width, sizeF.Height);
			RectangleF rectangleF = new RectangleF(layoutRectangle.Location, layoutRectangle.Size);
			rectangleF.Inflate(size);
			PointF[] points = new PointF[]
			{
				pointF,
				new PointF((float)((double)pointF.X - num2), rectangleF.Bottom),
				new PointF(rectangleF.Left + (float)num3, rectangleF.Bottom),
				new PointF(rectangleF.Left, rectangleF.Bottom - (float)num3),
				new PointF(rectangleF.Left, rectangleF.Top + (float)num3),
				new PointF(rectangleF.Left + (float)num3, rectangleF.Top),
				new PointF(rectangleF.Right - (float)num3, rectangleF.Top),
				new PointF(rectangleF.Right, rectangleF.Top + (float)num3),
				new PointF(rectangleF.Right, rectangleF.Bottom - (float)num3),
				new PointF(rectangleF.Right - (float)num3, rectangleF.Bottom),
				new PointF((float)((double)pointF.X + num2), rectangleF.Bottom)
			};
			e.Graphics.FillPolygon(this.fillBrush, points);
			e.Graphics.DrawPolygon(this.outlinePen, points);
			e.Graphics.DrawString(text, this.pinFont, this.textBrush, layoutRectangle);
		}
		private void DrawErrorMarkers(PositionAssociationView pav, PaintSpecification e)
		{
			this.DrawErrorPosition(pav, DisplayablePosition.ErrorMarker.AsContributor, this.errorContribPen, this.errorContribBrush, e);
			this.DrawErrorPosition(pav, DisplayablePosition.ErrorMarker.AsOutlier, this.errorOutlierPen, this.errorOutlierBrush, e);
		}
		private void DrawErrorPosition(PositionAssociationView pav, DisplayablePosition.ErrorMarker errorMarker, Pen pen, Brush brush, PaintSpecification e)
		{
			ErrorPosition errorPosition = pav.position.GetErrorPosition(errorMarker);
			if (errorPosition == null)
			{
				return;
			}
			PointF pointF = this.MapPositionToPoint(pav.position.pinPosition.latlon);
			PointF pointF2 = this.MapPositionToPoint(errorPosition.latlon);
			RectangleF rectangleF = new RectangleF((float)(e.ClipRectangle.X - e.ClipRectangle.Width * 2), (float)(e.ClipRectangle.Y - e.ClipRectangle.Height * 2), (float)(e.ClipRectangle.Width * 5), (float)(e.ClipRectangle.Height * 5));
			if (!rectangleF.Contains(pointF) || !rectangleF.Contains(pointF2))
			{
				return;
			}
			if (!pav.position.invertError)
			{
				e.Graphics.DrawLine(pen, pointF, pointF2);
				e.Graphics.FillEllipse(brush, pointF2.X - 6f, pointF2.Y - 6f, 12f, 12f);
				return;
			}
			e.Graphics.DrawEllipse(pen, pointF.X - 20f, pointF.Y - 20f, 40f, 40f);
		}
		protected override void OnPaint(PaintEventArgs e)
		{
			if (D.CustomPaintDisabled())
			{
				return;
			}
			this.PaintGraphics(new PaintSpecification(e.Graphics, e.ClipRectangle, base.Size, false), this.center().llz);
		}
		public void PaintPrintWindow(PaintSpecification e, int extraZoom)
		{
			this.PaintGraphics(e, new LatLonZoom(this.center().llz.lat, this.center().llz.lon, this.center().llz.zoom + extraZoom));
		}
		private void PaintGraphics(PaintSpecification e, LatLonZoom llz)
		{
			this.tilesRequired = 0;
			this.tilesAvailable = 0;
			this.asyncRequestGeneration++;
			if (this.baseLayer == null)
			{
				return;
			}
			InterestList interestList = this.activeTiles;
			this.activeTiles = new InterestList();
			e.ResetClip();
			e.Graphics.FillRectangle(new SolidBrush(Color.LightPink), new Rectangle(new Point(0, 0), e.Size));
			List<ViewerControl.PaintKit> list = new List<ViewerControl.PaintKit>();
			list.AddRange(this.AssembleLayer(e, llz, this.baseLayer, 0));
			int num = 1;
			foreach (IDisplayableSource current in this.alphaLayers)
			{
				list.AddRange(this.AssembleLayer(e, llz, current, num));
				num++;
			}
			this.activeTiles.Activate();
			this.PaintKits(e.Graphics, list);
			e.ResetClip();
			if (this.userRegionViewController != null)
			{
				e.ResetClip();
				this.userRegionViewController.Paint(e, llz, base.Size);
			}
			if (MapDrawingOption.IsEnabled(this.ShowCrosshairs))
			{
				Pen pen = new Pen(Color.Yellow);
				Pen[] array = new Pen[]
				{
					pen,
					new Pen(Color.Black)
					{
						DashStyle = DashStyle.Dash
					}
				};
				for (int i = 0; i < array.Length; i++)
				{
					Pen pen2 = array[i];
					e.Graphics.DrawLine(pen2, 0, base.Size.Height / 2, base.Size.Width, base.Size.Height / 2);
					e.Graphics.DrawLine(pen2, base.Size.Width / 2, 0, base.Size.Width / 2, base.Size.Height);
				}
			}
			if (MapDrawingOption.IsEnabled(this.ShowPushPins) && this.pinList != null)
			{
				List<PositionAssociationView> list2 = new List<PositionAssociationView>();
				list2.AddRange(this.pinList);
				list2.Sort(delegate(PositionAssociationView p0, PositionAssociationView p1)
				{
					double num2 = p1.position.pinPosition.lat - p0.position.pinPosition.lat;
					if (num2 != 0.0)
					{
						if (num2 <= 0.0)
						{
							return -1;
						}
						return 1;
					}
					else
					{
						double num3 = p1.position.pinPosition.lon - p0.position.pinPosition.lon;
						if (num3 == 0.0)
						{
							return 0;
						}
						if (num3 <= 0.0)
						{
							return -1;
						}
						return 1;
					}
				});
				foreach (PositionAssociationView current2 in list2)
				{
					this.DrawMarker(current2, e);
				}
			}
			if (interestList != null)
			{
				interestList.Dispose();
			}
			if (this.tilesRequired == 0 || this.tilesAvailable == this.tilesRequired)
			{
				this.displayProgressBar.Visible = false;
				return;
			}
			this.displayProgressBar.Visible = true;
			this.displayProgressBar.Minimum = 0;
			this.displayProgressBar.Maximum = this.tilesRequired;
			this.displayProgressBar.Value = this.tilesAvailable;
		}
		private void PaintKits(Graphics g, List<ViewerControl.PaintKit> kits)
		{
			g.CompositingMode = CompositingMode.SourceOver;
			foreach (ViewerControl.PaintKit current in kits)
			{
				foreach (ViewerControl.TilePaintClosure current2 in current.meatyParts)
				{
					current2.PaintTile(g, current.paintLocation);
					current2.Dispose();
				}
			}
			g.ResetClip();
			foreach (ViewerControl.PaintKit current3 in kits)
			{
				foreach (ViewerControl.TilePaintClosure current4 in current3.annotations)
				{
					current4.PaintTile(g, current3.paintLocation);
					current4.Dispose();
				}
			}
		}
		private List<ViewerControl.PaintKit> AssembleLayer(PaintSpecification e, LatLonZoom llz, IDisplayableSource tileSource, int stackOrder)
		{
			List<ViewerControl.PaintKit> list = new List<ViewerControl.PaintKit>();
			CoordinateSystemIfc defaultCoordinateSystem = tileSource.GetDefaultCoordinateSystem();
			TileDisplayDescriptorArray tileArrayDescriptor = defaultCoordinateSystem.GetTileArrayDescriptor(llz, e.Size);
			AsyncRef asyncRef;
			try
			{
				asyncRef = (AsyncRef)tileSource.GetUserBounds(null, (FutureFeatures)7).Realize("ViewerControl.PaintLayer boundsRef");
			}
			catch (Exception ex)
			{
				ViewerControl.MessagePainter item = new ViewerControl.MessagePainter(stackOrder * 12, BigDebugKnob.theKnob.debugFeaturesEnabled ? ex.ToString() : "X", stackOrder == 0);
				foreach (TileDisplayDescriptor current in tileArrayDescriptor)
				{
					list.Add(new ViewerControl.PaintKit(current.paintLocation)
					{
						annotations = 
						{
							item
						}
					});
				}
				return list;
			}
			Region clipRegion = null;
			if (asyncRef.present == null)
			{
				asyncRef.AddCallback(new AsyncRecord.CompleteCallback(this.BoundsRefAvailable));
				asyncRef.SetInterest(524290);
			}
			if ((this.ShowSourceCrop == null || this.ShowSourceCrop.Enabled) && asyncRef.present is IBoundsProvider)
			{
				clipRegion = ((IBoundsProvider)asyncRef.present).GetRenderRegion().GetClipRegion(defaultCoordinateSystem.GetUnclippedMapWindow(this.center().llz, e.Size), this.center().llz.zoom, defaultCoordinateSystem);
				this.UpdateUserRegion();
			}
			new PersistentInterest(asyncRef);
			int num = 0;
			foreach (TileDisplayDescriptor current2 in tileArrayDescriptor)
			{
				ViewerControl.PaintKit paintKit = new ViewerControl.PaintKit(current2.paintLocation);
				D.Sayf(10, "count {0} tdd {1}", new object[]
				{
					num,
					current2.tileAddress
				});
				num++;
				if (e.SynchronousTiles)
				{
					D.Sayf(0, "PaintLayer({0}, tdd.ta={1})", new object[]
					{
						tileSource.GetHashCode(),
						current2.tileAddress
					});
				}
				bool arg_1F5_0 = e.SynchronousTiles;
				Present present = tileSource.GetImagePrototype(null, (FutureFeatures)15).Curry(new ParamDict(new object[]
				{
					TermName.TileAddress,
					current2.tileAddress
				})).Realize("ViewerControl.PaintLayer imageAsyncRef");
				AsyncRef asyncRef2 = (AsyncRef)present;
				Rectangle rectangle = Rectangle.Intersect(e.ClipRectangle, current2.paintLocation);
				int interest = rectangle.Height * rectangle.Width + 524296;
				asyncRef2.SetInterest(interest);
				if (asyncRef2.present == null)
				{
					ViewerControl.AsyncNotifier @object = new ViewerControl.AsyncNotifier(this);
					asyncRef2.AddCallback(new AsyncRecord.CompleteCallback(@object.AsyncRecordComplete));
				}
				this.activeTiles.Add(asyncRef2);
				asyncRef2 = (AsyncRef)asyncRef2.Duplicate("ViewerControl.PaintLayer");
				if (e.SynchronousTiles)
				{
					D.Assert(false, "unimpl");
				}
				if (asyncRef2.present == null)
				{
					D.Assert(!e.SynchronousTiles);
				}
				bool flag;
				if (asyncRef2.present != null && asyncRef2.present is ImageRef)
				{
					flag = false;
					ImageRef imageRef = (ImageRef)asyncRef2.present.Duplicate("tpc");
					paintKit.meatyParts.Add(new ViewerControl.ImagePainter(imageRef, clipRegion));
				}
				else
				{
					if (asyncRef2.present != null && asyncRef2.present is BeyondImageBounds)
					{
						flag = false;
					}
					else
					{
						if (asyncRef2.present != null && asyncRef2.present is PresentFailureCode)
						{
							flag = false;
							PresentFailureCode presentFailureCode = (PresentFailureCode)asyncRef2.present;
							ViewerControl.MessagePainter item2 = new ViewerControl.MessagePainter(stackOrder * 12, BigDebugKnob.theKnob.debugFeaturesEnabled ? StringUtils.breakLines(presentFailureCode.ToString()) : "X", stackOrder == 0);
							paintKit.annotations.Add(item2);
						}
						else
						{
							flag = true;
							ViewerControl.MessagePainter item3 = new ViewerControl.MessagePainter(stackOrder * 12, stackOrder.ToString(), stackOrder == 0);
							if (stackOrder == 0)
							{
								paintKit.meatyParts.Add(item3);
							}
							else
							{
								paintKit.annotations.Add(item3);
							}
						}
					}
				}
				this.tilesRequired++;
				if (!flag)
				{
					this.tilesAvailable++;
				}
				if ((flag && stackOrder == 0) || MapDrawingOption.IsEnabled(this.ShowTileBoundaries))
				{
					paintKit.annotations.Add(new ViewerControl.TileBoundaryPainter());
				}
				if (MapDrawingOption.IsEnabled(this.ShowTileNames))
				{
					paintKit.annotations.Add(new ViewerControl.TileNamePainter(current2.tileAddress.ToString()));
				}
				asyncRef2.Dispose();
				list.Add(paintKit);
			}
			return list;
		}
		private void BoundsRefAvailable(AsyncRef asyncRef)
		{
			base.Invalidate();
		}
		private void UpdateUserRegion()
		{
			if (this.userRegionViewController == null && this.latentRegionHolder != null)
			{
				this.userRegionViewController = new UserRegionViewController(this.GetCoordinateSystem(), this, this.latentRegionHolder, this.baseLayer);
				this.InvalidateView();
			}
		}
		public void PositionUpdated(LatLonZoom llz)
		{
			this.llzBox.PositionChanged(llz);
			base.Invalidate();
		}
		public void ForceInteractiveUpdate()
		{
			base.Update();
		}
		public void InvalidateView()
		{
			base.Invalidate();
		}
		public void InvalidatePipeline()
		{
			try
			{
				foreach (DisplayableSourceCache current in this.alphaLayers)
				{
					current.Flush();
				}
				if (this.baseLayer != null)
				{
					this.baseLayer.Flush();
				}
				base.Invalidate();
			}
			catch (InvalidOperationException)
			{
			}
		}
		public void AddLayer(IDisplayableSource warpedMapTileSource)
		{
			if (this.baseLayer == null)
			{
				this.SetBaseLayer(warpedMapTileSource);
				return;
			}
			this.AddAlphaLayer(warpedMapTileSource);
		}
		public Pixel GetBaseLayerCenterPixel()
		{
			IDisplayableSource displayableSource = this.baseLayer;
			CoordinateSystemIfc defaultCoordinateSystem = displayableSource.GetDefaultCoordinateSystem();
			TileDisplayDescriptorArray tileArrayDescriptor = defaultCoordinateSystem.GetTileArrayDescriptor(this.center().llz, base.Size);
			int num = base.Size.Width / 2;
			int num2 = base.Size.Height / 2;
			foreach (TileDisplayDescriptor current in tileArrayDescriptor)
			{
				Rectangle paintLocation = current.paintLocation;
				if (paintLocation.Left <= num)
				{
					Rectangle paintLocation2 = current.paintLocation;
					if (paintLocation2.Right > num)
					{
						Rectangle paintLocation3 = current.paintLocation;
						if (paintLocation3.Top <= num2)
						{
							Rectangle paintLocation4 = current.paintLocation;
							if (paintLocation4.Bottom > num2)
							{
								int arg_D1_0 = num;
								Rectangle paintLocation5 = current.paintLocation;
								int x = arg_D1_0 - paintLocation5.Left;
								int arg_E6_0 = num2;
								Rectangle paintLocation6 = current.paintLocation;
								int y = arg_E6_0 - paintLocation6.Top;
								Present present = displayableSource.GetImagePrototype(null, (FutureFeatures)19).Curry(new ParamDict(new object[]
								{
									TermName.TileAddress,
									current.tileAddress
								})).Realize("ViewerControl.GetBaseLayerCenterPixel imageRef");
								Pixel result;
								if (!(present is ImageRef))
								{
									result = new UndefinedPixel();
									return result;
								}
								ImageRef imageRef = (ImageRef)present;
								GDIBigLockedImage image;
								Monitor.Enter(image = imageRef.image);
								Pixel pixel2;
								try
								{
									Image image2 = imageRef.image.IPromiseIAmHoldingGDISLockSoPleaseGiveMeTheImage();
									Bitmap bitmap = (Bitmap)image2;
									Color pixel = bitmap.GetPixel(x, y);
									pixel2 = new Pixel(pixel);
								}
								finally
								{
									Monitor.Exit(image);
								}
								imageRef.Dispose();
								result = pixel2;
								return result;
							}
						}
					}
				}
			}
			return new UndefinedPixel();
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
            this.zoomOutButton = new System.Windows.Forms.Button();
            this.zoomInButton = new System.Windows.Forms.Button();
            this.creditsTextBox = new System.Windows.Forms.TextBox();
            this.zenButton = new System.Windows.Forms.Button();
            this.displayProgressBar = new System.Windows.Forms.ProgressBar();
            this.llzBox = new MSR.CVE.BackMaker.LLZBox();
            this.SuspendLayout();
            // 
            // zoomOutButton
            // 
            this.zoomOutButton.Location = new System.Drawing.Point(242, 39);
            this.zoomOutButton.Name = "zoomOutButton";
            this.zoomOutButton.Size = new System.Drawing.Size(82, 23);
            this.zoomOutButton.TabIndex = 9;
            this.zoomOutButton.Text = "Zoom Out";
            this.zoomOutButton.Click += new System.EventHandler(this.zoomOutButton_Click);
            // 
            // zoomInButton
            // 
            this.zoomInButton.Location = new System.Drawing.Point(242, 68);
            this.zoomInButton.Name = "zoomInButton";
            this.zoomInButton.Size = new System.Drawing.Size(82, 23);
            this.zoomInButton.TabIndex = 10;
            this.zoomInButton.Text = "Zoom In";
            this.zoomInButton.Click += new System.EventHandler(this.zoomInButton_Click);
            // 
            // creditsTextBox
            // 
            this.creditsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.creditsTextBox.Location = new System.Drawing.Point(330, 34);
            this.creditsTextBox.Multiline = true;
            this.creditsTextBox.Name = "creditsTextBox";
            this.creditsTextBox.ReadOnly = true;
            this.creditsTextBox.Size = new System.Drawing.Size(115, 32);
            this.creditsTextBox.TabIndex = 12;
            this.creditsTextBox.Visible = false;
            // 
            // zenButton
            // 
            this.zenButton.Location = new System.Drawing.Point(242, 10);
            this.zenButton.Name = "zenButton";
            this.zenButton.Size = new System.Drawing.Size(82, 23);
            this.zenButton.TabIndex = 13;
            this.zenButton.Text = "zenButton";
            this.zenButton.UseVisualStyleBackColor = true;
            // 
            // displayProgressBar
            // 
            this.displayProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.displayProgressBar.Location = new System.Drawing.Point(330, 14);
            this.displayProgressBar.Name = "displayProgressBar";
            this.displayProgressBar.Size = new System.Drawing.Size(115, 19);
            this.displayProgressBar.TabIndex = 14;
            // 
            // llzBox
            // 
            this.llzBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.llzBox.Location = new System.Drawing.Point(0, 0);
            this.llzBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.llzBox.Name = "llzBox";
            this.llzBox.Size = new System.Drawing.Size(456, 95);
            this.llzBox.TabIndex = 11;
            // 
            // ViewerControl
            // 
            this.Controls.Add(this.creditsTextBox);
            this.Controls.Add(this.zoomInButton);
            this.Controls.Add(this.zoomOutButton);
            this.Controls.Add(this.displayProgressBar);
            this.Controls.Add(this.zenButton);
            this.Controls.Add(this.llzBox);
            this.Name = "ViewerControl";
            this.Size = new System.Drawing.Size(456, 615);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		Size ViewerControlIfcSize
		{
            get
            {
                return base.Size;
            }
		}
	}
}
