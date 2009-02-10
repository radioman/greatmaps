using System.Drawing;
using System.IO;

using GMapNET;
using GMapNET.Internals;

namespace System.Windows.Forms
{
   public partial class GMap : UserControl, IGControl
   {
      // internal core
      readonly Core Core = new Core();

      delegate void MethodInvoker();

      readonly Pen routePen = new Pen(Color.MidnightBlue);
      //readonly Pen tooltipPen = new Pen(Color.FromArgb(140, Color.MidnightBlue));
      //readonly Color tooltipBg = Color.FromArgb(140, Color.AliceBlue);
      //readonly int SourceCopy = (int) CopyPixelOperation.SourceCopy;
      readonly Font gFont = new Font(FontFamily.GenericSansSerif, 7, FontStyle.Regular);
      StringFormat tooltipFormat = new StringFormat();

      GMapNET.Rectangle region;

      public GMap()
      {
         {
            Purity.Instance.ImageProxy = new WindowsFormsImageProxy();

            //this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            //this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            //this.SetStyle(ControlStyles.UserPaint, true);
            //this.SetStyle(ControlStyles.Opaque, true);

            // to know when to invalidate
            Core.OnNeedInvalidation += new NeedInvalidation(Core_OnNeedInvalidation);

            region = new GMapNET.Rectangle(-50, -50, Size.Width+100, Size.Height+100);

            RenderMode = RenderMode.GDI_PLUS;

            //this.Region = new Region(new Rectangle(-50, -50, Size.Width+100, Size.Height+100));

            //routePen.LineJoin = LineJoin.Round;
            //routePen.Width = 5;

            //tooltipPen.Width = 2;
            //tooltipPen.LineJoin = LineJoin.Round;
            //tooltipPen.StartCap = LineCap.RoundAnchor;

            tooltipFormat.Alignment     = StringAlignment.Center;
            tooltipFormat.LineAlignment = StringAlignment.Center;
         }
      }

      /// <summary>
      /// on core needs invalidation
      /// </summary>
      void Core_OnNeedInvalidation()
      {
         if(this.InvokeRequired)
         {
            MethodInvoker m = delegate
            {
               Invalidate();
            };
            this.Invoke(m);
         }
         else
         {
            Invalidate();
         }
      }

      /// <summary>
      /// render map in GDI+
      /// </summary>
      /// <param name="g"></param>
      void DrawMapGDIplus(Graphics g)
      {
         for(int i = -(Core.sizeOfMapArea.Width + Core.centerTileXYOffset.X); i < (Core.sizeOfMapArea.Width - Core.centerTileXYOffset.X); i++)
         {
            for(int j = -(Core.sizeOfMapArea.Height + Core.centerTileXYOffset.Y); j < (Core.sizeOfMapArea.Height - Core.centerTileXYOffset.Y); j++)
            {
               Core.tilePoint = CurrentPositionGTile;
               Core.tilePoint.X += i;
               Core.tilePoint.Y += j;

               Tile t = Core.Matrix[Core.tilePoint];
               if(t != null) // debug center tile add: && Core.tilePoint != Core.centerTileXYLocation
               {
                  Core.tileRect.X = Core.tilePoint.X*Core.tileRect.Width;
                  Core.tileRect.Y = Core.tilePoint.Y*Core.tileRect.Height;
                  Core.tileRect.Offset(Core.renderOffset);

                  if(region.IntersectsWith(Core.tileRect))
                  {
                     foreach(WindowsFormsImage img in t.Overlays)
                     {
                        if(img != null && img.Img != null)
                        {
                           g.DrawImage(img.Img, Core.tileRect.X, Core.tileRect.Y);
                        }
                     }
                  }
               }
            }
         }
      }

      /// <summary>
      /// override to draws custom current marker
      /// </summary>
      /// <param name="g"></param>
      /// <param name="x"></param>
      /// <param name="y"></param>
      protected virtual void DrawCurrentMarker(Graphics g, int x, int y)
      {

      }

      #region UserControl Events

      //protected bool DesignModeInConstruct
      //{
      //   get
      //   {
      //      return (LicenseManager.UsageMode == LicenseUsageMode.Designtime);
      //   }
      //}

      //protected override void OnLoad(EventArgs e)
      //{
      //   base.OnLoad(e);

      //   Core.StartSystem();
      //}

      protected override void OnPaintBackground(PaintEventArgs e)
      {
         //{
         //   e.Graphics.FillRectangle(Brushes.Gray, 0, 0, Width, Height);
         //   Point p = new Point(Width/2, Height/2);
         //   e.Graphics.DrawImageUnscaled(GMapNET.Properties.Resources.shadow50, p.X-10, p.Y-34);
         //   e.Graphics.DrawImageUnscaled(GMapNET.Properties.Resources.marker, p.X-10, p.Y-34);
         //}
      }

      protected override void OnPaint(PaintEventArgs e)
      {
         switch(RenderMode)
         {
            //case RenderMode.GDI:
            //{
            //   try
            //   {
            //      DrawMapGDI(e.Graphics);
            //   }
            //   catch // drop to gdi+ redering if smth
            //   {
            //      RenderMode = RenderMode.GDI_PLUS;
            //   }
            //}
            //break;

            case RenderMode.GDI_PLUS:
            {
               DrawMapGDIplus(e.Graphics);
            }
            break;
         }

         if(RoutesEnabled && (!IsDragging))
         {
            //DrawRoutes(e.Graphics);
         }

         if(MarkersEnabled && (!IsDragging))
         {
            //DrawMarkers(e.Graphics);
         }

         if(CurrentMarkerEnabled)
         {
            //DrawCurrentMarker(e.Graphics);
         }

         #region -- copyright --

         switch(Core.MapType)
         {
            case GMapType.GoogleMap:
            case GMapType.GoogleSatellite:
            case GMapType.GoogleLabels:
            case GMapType.GoogleTerrain:
            {
               //e.Graphics.DrawString(Core.googleCopyright, gFont, Brushes.Navy, 3, Height - gFont.Height - 5);
            }
            break;

            case GMapType.OpenStreetMap:
            case GMapType.OpenStreetOsm:
            {
               //e.Graphics.DrawString(Core.openStreetMapCopyright, gFont, Brushes.Navy, 3, Height - gFont.Height - 5);
            }
            break;

            case GMapType.YahooMap:
            case GMapType.YahooSatellite:
            case GMapType.YahooLabels:
            {
               //e.Graphics.DrawString(Core.yahooMapCopyright, gFont, Brushes.Navy, 3, Height - gFont.Height - 5);
            }
            break;
         }

         #endregion
      }

      protected override void OnHandleCreated(EventArgs e)
      {
         Core.StartSystem();

         base.OnHandleCreated(e);
      }

      protected override void OnResize(EventArgs e)
      {
         base.OnResize(e);

         if(this.Visible)
         {
            Core.sizeOfMapArea = new GMapNET.Size(Bounds.Width, Bounds.Height);
            Core.sizeOfMapArea.Height /= GMaps.Instance.TileSize.Height;
            Core.sizeOfMapArea.Width /= GMaps.Instance.TileSize.Width;
            Core.sizeOfMapArea.Height += 1;
            Core.sizeOfMapArea.Width += 1;

            Core.sizeOfMapArea.Width = Core.sizeOfMapArea.Width/2 + 2;
            Core.sizeOfMapArea.Height = Core.sizeOfMapArea.Height/2 + 2;

            // 50px outside control
            region = new GMapNET.Rectangle(-50, -50, Size.Width+100, Size.Height+100);

            Core.OnMapSizeChanged(Width, Height);

            this.Invalidate();
         }
      }

      //protected override void OnSizeChanged(EventArgs e)
      //{
      //   if(DesignMode)
      //      return;

      //   Core.sizeOfMapArea = Bounds.Size;
      //   Core.sizeOfMapArea.Height /= GMaps.Instance.TileSize.Height;
      //   Core.sizeOfMapArea.Width /= GMaps.Instance.TileSize.Width;
      //   Core.sizeOfMapArea.Height += 1;
      //   Core.sizeOfMapArea.Width += 1;

      //   Core.sizeOfMapArea.Width = Core.sizeOfMapArea.Width/2 + 2;
      //   Core.sizeOfMapArea.Height = Core.sizeOfMapArea.Height/2 + 2;

      //   // 50px outside control
      //   this.Region = new Region(new Rectangle(-50, -50, Size.Width+100, Size.Height+100));

      //   Core.OnMapSizeChanged(Width, Height);

      //   base.OnSizeChanged(e);
      //}

      protected override void OnMouseDown(MouseEventArgs e)
      {
         Core.mouseDown.X = e.X;
         Core.mouseDown.Y = e.Y;

         if(e.Button == MouseButtons.Left)
         {
            if(CurrentMarkerEnabled && !IsMouseOverMarker)
            {
               SetCurrentPositionOnly(e.X - Core.renderOffset.X, e.Y - Core.renderOffset.Y);

               if(Core.MouseVisible)
               {
                  //this.Cursor = System.Windows.Forms.Cursors.Default;
                  //Cursor.Hide();
                  Core.MouseVisible = false;
               }

               Core.BeginDrag(Core.mouseDown);
            }
         }
         else if(e.Button == MouseButtons.Right)
         {
            if(CanDragMap)
            {
               //this.Cursor = System.Windows.Forms.Cursors.SizeAll;

               Core.BeginDrag(Core.mouseDown);
            }
         }

         this.Invalidate();

         base.OnMouseDown(e);
      }

      protected override void OnMouseUp(MouseEventArgs e)
      {
         if(Core.IsDragging)
         {
            Core.EndDrag();

            //this.Cursor = System.Windows.Forms.Cursors.Default;

            if(!Core.MouseVisible)
            {
               //Cursor.Show();
               Core.MouseVisible = true;
            }
         }

         base.OnMouseUp(e);
      }

      //protected override void OnMouseClick(MouseEventArgs e)
      //{
      //   if(e.Button == MouseButtons.Left)
      //   {
      //      Core.CheckIfClickOnMarker(e.Location);
      //   }

      //   base.OnMouseClick(e);
      //}

      protected override void OnMouseMove(MouseEventArgs e)
      {
         if(Core.IsDragging)
         {
            Core.mouseCurrent.X = e.X;
            Core.mouseCurrent.Y = e.Y;

            if(e.Button == MouseButtons.Right)
            {
               Core.Drag(Core.mouseCurrent);
            }
            else if(e.Button == MouseButtons.Left)
            {
               if(CurrentMarkerEnabled)
               {
                  SetCurrentPositionOnly(e.X - Core.renderOffset.X, e.Y - Core.renderOffset.Y);
                  this.Invalidate();
               }
            }
         }
         else
         {
            /*
            lock(Core.markers)
            {
               for(int i = 0; i < Core.markers.Count; i++)
               {
                  Marker m = Core.markers[i];
                  if(m.Visible)
                  {
                     Rectangle rc1 = Core.GetRectForMarker(e.Location, m);

                     using(Region rg = new Region(rc1))
                     {
                        if(rg.IsVisible(GMaps.Instance.FromLatLngToPixel(m.Position, Zoom)))
                        {
                           this.Cursor = System.Windows.Forms.Cursors.Hand;
                           m.IsMouseOver = true;
                           Core.isMouseOverMarker = true;
                           Invalidate(false);

                           if(Core.OnMarkerEnter != null)
                           {
                              Core.OnMarkerEnter(m);
                           }
                        }
                        else if(m.IsMouseOver)
                        {
                           this.Cursor = System.Windows.Forms.Cursors.Default;
                           m.IsMouseOver = false;
                           Core.isMouseOverMarker = false;
                           Invalidate(false);

                           if(Core.OnMarkerLeave != null)
                           {
                              Core.OnMarkerLeave(m);
                           }
                        }
                     }
                  }
               }
            }
             */
         }
         base.OnMouseMove(e);
      }
      #endregion

      #region IGControl Members

      public void ReloadMap()
      {
         Core.ReloadMap();
      }

      public void GoToCurrentPosition()
      {
         Core.GoToCurrentPosition();
      }

      public bool ZoomAndCenterMarkers()
      {
         return Core.ZoomAndCenterMarkers();
      }

      public bool SetCurrentPositionByKeywords(string keys)
      {
         return Core.SetCurrentPositionByKeywords(keys);
      }

      public void SetCurrentMarkersVisibility(bool visible)
      {
         Core.SetCurrentMarkersVisibility(visible);
      }

      public void SetCurrentMarkersTooltipMode(MarkerTooltipMode mode)
      {
         Core.SetCurrentMarkersTooltipMode(mode);
      }

      public PointLatLng FromLocalToLatLng(int x, int y)
      {
         return Core.FromLocalToLatLng(x, y);
      }

      public GMapNET.Point FromLatLngToLocal(PointLatLng point)
      {
         return Core.FromLatLngToLocal(point);
      }

      public RectLatLng GetRectOfAllMarkers()
      {
         return Core.GetRectOfAllMarkers();
      }

      public void AddRoute(Route item)
      {
         Core.AddRoute(item);
      }

      public void RemoveRoute(Route item)
      {
         Core.RemoveRoute(item);
      }

      public void ClearAllRoutes()
      {
         Core.ClearAllRoutes();
      }

      public void AddMarker(Marker item)
      {
         Core.AddMarker(item);
      }

      public void RemoveMarker(Marker item)
      {
         Core.RemoveMarker(item);
      }

      public void ClearAllMarkers()
      {
         Core.ClearAllMarkers();
      }

      public void SetCurrentPositionOnly(int x, int y)
      {
         Core.SetCurrentPositionOnly(x, y);
      }

      public void SetCurrentPositionOnly(PointLatLng point)
      {
         Core.SetCurrentPositionOnly(point);
      }

      public bool ShowExportDialog()
      {
         return false;
      }

      public bool ShowImportDialog()
      {
         return false;
      }

      //[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
      public int Zoom
      {
         get
         {
            return Core.Zoom;
         }
         set
         {
            Core.Zoom = value;
         }
      }

      public PointLatLng CurrentPosition
      {
         get
         {
            return Core.CurrentPosition;
         }
         set
         {
            Core.CurrentPosition = value;
         }
      }

      public GMapNET.Point CurrentPositionGPixel
      {
         get
         {
            return Core.CurrentPositionGPixel;
         }
      }

      public GMapNET.Point CurrentPositionGTile
      {
         get
         {
            return Core.CurrentPositionGTile;
         }
      }

      //[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
      public string CacheLocation
      {
         get
         {
            return Cache.Instance.CacheLocation;
         }
         set
         {
            Cache.Instance.CacheLocation = value;
         }
      }

      public long TotalTiles
      {
         get
         {
            return Core.TotalTiles;
         }
      }

      public bool IsDragging
      {
         get
         {
            return Core.IsDragging;
         }
      }

      public bool IsMouseOverMarker
      {
         get
         {
            return Core.IsMouseOverMarker;
         }
      }

      public bool CurrentMarkerEnabled
      {
         get
         {
            return Core.CurrentMarkerEnabled;
         }
         set
         {
            Core.CurrentMarkerEnabled = value;
         }
      }

      public RectLatLng CurrentViewArea
      {
         get
         {
            return Core.CurrentViewArea;
         }
      }

      public GMapType MapType
      {
         get
         {
            return Core.MapType;
         }
         set
         {
            Core.MapType = value;
         }
      }

      public bool RoutesEnabled
      {
         get
         {
            return Core.RoutesEnabled;
         }
         set
         {
            Core.RoutesEnabled = value;
         }
      }

      public bool MarkersEnabled
      {
         get
         {
            return Core.MarkersEnabled;
         }
         set
         {
            Core.MarkersEnabled = value;
         }
      }

      public bool CanDragMap
      {
         get
         {
            return Core.CanDragMap;
         }
         set
         {
            Core.CanDragMap = value;
         }
      }

      public CurrentMarkerType CurrentMarkerStyle
      {
         get
         {
            return Core.CurrentMarkerStyle;
         }
         set
         {
            Core.CurrentMarkerStyle = value;
         }
      }

      public RenderMode RenderMode
      {
         get
         {
            return Core.RenderMode;
         }
         internal set
         {
            Core.RenderMode = value;
         }
      }

      #endregion

      #region IGControl event Members

      public event CurrentPositionChanged OnCurrentPositionChanged
      {
         add
         {
            Core.OnCurrentPositionChanged += value;
         }
         remove
         {
            Core.OnCurrentPositionChanged -= value;
         }
      }

      public event TileLoadComplete OnTileLoadComplete
      {
         add
         {
            Core.OnTileLoadComplete += value;
         }
         remove
         {
            Core.OnTileLoadComplete -= value;
         }
      }

      public event TileLoadStart OnTileLoadStart
      {
         add
         {
            Core.OnTileLoadStart += value;
         }
         remove
         {
            Core.OnTileLoadStart -= value;
         }
      }

      public event MarkerClick OnMarkerClick
      {
         add
         {
            Core.OnMarkerClick += value;
         }
         remove
         {
            Core.OnMarkerClick -= value;
         }
      }

      public event MarkerEnter OnMarkerEnter
      {
         add
         {
            Core.OnMarkerEnter += value;
         }
         remove
         {
            Core.OnMarkerEnter -= value;
         }
      }

      public event MarkerLeave OnMarkerLeave
      {
         add
         {
            Core.OnMarkerLeave += value;
         }
         remove
         {
            Core.OnMarkerLeave -= value;
         }
      }

      #endregion
   }

   #region -- purity control --
   internal class WindowsFormsImage : PureImage
   {
      public System.Drawing.Image Img;

      public override IntPtr GetHbitmap()
      {
         if(Img != null)
         {
            using(Bitmap bitmap = new Bitmap(Img))
            {
               return bitmap.GetHbitmap();
            }
         }
         return IntPtr.Zero;
      }

      public override object Clone()
      {
         if(Img != null)
         {
            WindowsFormsImage ret = new WindowsFormsImage();
            ret.Img = Img.Clone() as Image;
            return ret;
         }
         return null;
      }

      public override void Dispose()
      {
         if(Img != null)
         {
            Img.Dispose();
            Img = null;
         }
      }
   }

   internal class WindowsFormsImageProxy : PureImageProxy
   {
      public override PureImage FromStream(Stream stream)
      {
         WindowsFormsImage ret = null;
         try
         {
            Image m = new Bitmap(stream);
            ret = new WindowsFormsImage();
            ret.Img = m;
         }
         catch
         {
            ret = null;
         }
         return ret;
      }

      public override bool Save(Stream stream, GMapNET.PureImage image)
      {
         WindowsFormsImage ret = image as WindowsFormsImage;
         bool ok = true;

         if(ret.Img != null)
         {
            {
               // try png
               try
               {
                  ret.Img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
               }
               catch
               {
                  ok = false;
               }

               // try jpeg
               if(!ok)
               {
                  ok = true;
                  try
                  {
                     stream.Seek(0, SeekOrigin.Begin);
                     ret.Img.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                  }
                  catch
                  {
                     ok = false;
                  }
               }
            }
         }
         else
         {
            ok = false;
         }

         return ok;
      }
   }
   #endregion
}
