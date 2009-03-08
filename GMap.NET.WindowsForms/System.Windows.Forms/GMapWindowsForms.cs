using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Collections.ObjectModel;

using GMapNET;
using GMapNET.Internals;

namespace System.Windows.Forms
{
   public partial class GMap : UserControl, IGControl
   {
      readonly Core Core = new Core();
      readonly Pen routePen = new Pen(Color.MidnightBlue);
      readonly Pen tooltipPen = new Pen(Color.FromArgb(140, Color.MidnightBlue));
      readonly Color tooltipBg = Color.FromArgb(140, Color.AliceBlue);
      readonly Font gFont = new Font(FontFamily.GenericSansSerif, 7, FontStyle.Regular);
      readonly StringFormat tooltipFormat = new StringFormat();
      GMapNET.Rectangle region;

      /// <summary>
      /// list of markers, should be thread safe
      /// </summary>
      public readonly ObservableCollectionThreadSafe<GMapNET.MapObject> Markers = new ObservableCollectionThreadSafe<GMapNET.MapObject>();
      public readonly ObservableCollectionThreadSafe<GMapNET.MapRoute> Routes = new ObservableCollectionThreadSafe<GMapNET.MapRoute>();

      public GMap()
      {
         if(!DesignModeInConstruct)
         {
            Purity.Instance.ImageProxy = new WindowsFormsImageProxy();

            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.Opaque, true);

            RenderMode = RenderMode.GDI_PLUS;

            // to know when to invalidate
            Core.OnNeedInvalidation += new NeedInvalidation(Core_OnNeedInvalidation);
            Core.OnMapDrag += new MapDrag(GMap_OnMapDrag);

            Markers.CollectionChanged += new NotifyCollectionChangedEventHandler(Markers_CollectionChanged);
            Routes.CollectionChanged += new NotifyCollectionChangedEventHandler(Routes_CollectionChanged);

            region = new GMapNET.Rectangle(-50, -50, Size.Width+100, Size.Height+100);

            routePen.LineJoin = LineJoin.Round;
            routePen.Width = 5;

            tooltipPen.Width = 2;
            tooltipPen.LineJoin = LineJoin.Round;
            tooltipPen.StartCap = LineCap.RoundAnchor;

            tooltipFormat.Alignment     = StringAlignment.Center;
            tooltipFormat.LineAlignment = StringAlignment.Center;
         }
      }

      void Routes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
      {
         if(e.NewItems != null)
         {
            foreach(MapRoute obj in e.NewItems)
            {
               UpdateRouteLocalPosition(obj);
            }
         }

         Core_OnNeedInvalidation();
      }

      void Markers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
      {
         if(e.NewItems != null)
         {
            foreach(MapObject obj in e.NewItems)
            {
               UpdateMarkerLocalPosition(obj);
            }
         }

         Core_OnNeedInvalidation();
      }

      void GMap_OnMapDrag()
      {
         foreach(MapObject obj in Markers)
         {
            UpdateMarkerLocalPosition(obj);
         }

         foreach(MapRoute obj in Routes)
         {
            UpdateRouteLocalPosition(obj);
         }
      }

      /// <summary>
      /// updates markers local position
      /// </summary>
      /// <param name="marker"></param>
      public void UpdateMarkerLocalPosition(MapObject marker)
      {
         GMapNET.Point p = GMaps.Instance.FromLatLngToPixel(marker.Position, Core.Zoom);
         p.Offset(Core.renderOffset);
         marker.LocalPosition = p;
      }

      /// <summary>
      /// updates route local position
      /// </summary>
      /// <param name="route"></param>
      public void UpdateRouteLocalPosition(MapRoute route)
      {
         route.LocalPoints.Clear();

         foreach(GMapNET.PointLatLng pg in route.Points)
         {
            GMapNET.Point p = GMaps.Instance.FromLatLngToPixel(pg, Core.Zoom);
            p.Offset(Core.renderOffset);
            route.LocalPoints.Add(p);
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
               Invalidate(false);
            };
            this.Invoke(m);
         }
         else
         {
            Invalidate(false);
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
                           g.DrawImageUnscaled(img.Img, Core.tileRect.X, Core.tileRect.Y);
                        }
                     }
                  }
               }
            }
         }
      }

      /// <summary>
      /// draws tooltip, override to draw custom
      /// </summary>
      /// <param name="g"></param>
      /// <param name="pos"></param>
      protected virtual void DrawToolTip(Graphics g, GMapMarker m, int x, int y)
      {
         GraphicsState s = g.Save();
         g.SmoothingMode = SmoothingMode.AntiAlias;

         //Size st = g.MeasureString(m.Text, TooltipFont).ToSize();
         //Rectangle rect = new Rectangle(x, y, st.Width+TooltipTextPadding.Width, st.Height+TooltipTextPadding.Height);
         //rect.Offset(m.ToolTipOffset);

         //g.DrawLine(tooltipPen, x, y, rect.X + rect.Width/2, rect.Y + rect.Height/2);
         //g.FillRectangle(Brushes.AliceBlue, rect);
         //g.DrawRectangle(tooltipPen, rect);
         //g.DrawString(m.Text, TooltipFont, Brushes.Navy, rect, tooltipFormat);

         g.Restore(s);
      }

      /// <summary>
      /// draw routes
      /// </summary>
      /// <param name="g"></param>
      void DrawRoutes(Graphics g)
      {
         GraphicsState st = g.Save();
         g.SmoothingMode = SmoothingMode.AntiAlias;

         foreach(GMapRoute r in Routes)
         {
            routePen.Color = r.Color;

            using(GraphicsPath rp = new GraphicsPath())
            {
               for(int i = 0; i < r.LocalPoints.Count; i++)
               {
                  GMapNET.Point p2 = r.LocalPoints[i];

                  if(i == 0)
                  {
                     rp.AddLine(p2.X, p2.Y, p2.X, p2.Y);
                  }
                  else
                  {
                     System.Drawing.PointF p = rp.GetLastPoint();
                     rp.AddLine(p.X, p.Y, p2.X, p2.Y);
                  }
               }

               if(rp.PointCount > 0)
               {
                  g.DrawPath(routePen, rp);
               }
            }
         }

         g.Restore(st);
      }

      /// <summary>
      /// sets to max zoom to fit all markers and centers them in map
      /// </summary>
      public bool ZoomAndCenterMarkers()
      {
         RectLatLng rect = GetRectOfAllMarkers();
         if(rect != RectLatLng.Empty)
         {
            int maxZoom = Core.GetMaxZoomToFitRect(rect);
            if(maxZoom > 0)
            {
               PointLatLng center = new PointLatLng(rect.Lat-(rect.HeightLat/2), rect.Lng+(rect.WidthLng/2));
               CurrentPosition = center;

               if(Zoom != maxZoom)
               {
                  Zoom = maxZoom;
               }

               GoToCurrentPosition();

               return true;
            }
         }

         return false;
      }

      /// <summary>
      /// gets rectangle with all objects inside
      /// </summary>
      /// <returns></returns>
      public RectLatLng GetRectOfAllMarkers()
      {
         RectLatLng ret = RectLatLng.Empty;

         {
            if(Markers.Count > 0)
            {
               double left = double.MaxValue;
               double top = double.MinValue;
               double right = double.MinValue;
               double bottom = double.MaxValue;

               foreach(MapObject m in Markers)
               {
                  // left
                  if(m.Position.Lng < left)
                  {
                     left = m.Position.Lng;
                  }

                  // top
                  if(m.Position.Lat > top)
                  {
                     top = m.Position.Lat;
                  }

                  // right
                  if(m.Position.Lng > right)
                  {
                     right = m.Position.Lng;
                  }

                  // bottom
                  if(m.Position.Lat < bottom)
                  {
                     bottom = m.Position.Lat;
                  }
               }

               ret = RectLatLng.FromLTRB(left, top, right, bottom);
            }
         }

         return ret;
      }

      #region UserControl Events

      protected bool DesignModeInConstruct
      {
         get
         {
            return (LicenseManager.UsageMode == LicenseUsageMode.Designtime);
         }
      }

      protected override void OnLoad(EventArgs e)
      {
         base.OnLoad(e);

         Core.StartSystem();
      }

      protected override void OnPaintBackground(PaintEventArgs e)
      {
         if(DesignMode)
         {
            e.Graphics.FillRectangle(Brushes.Gray, 0, 0, Width, Height);
         }
      }

      protected override void OnPaint(PaintEventArgs e)
      {
         switch(RenderMode)
         {
            case RenderMode.GDI_PLUS:
            {
               DrawMapGDIplus(e.Graphics);
            }
            break;
         }

         if(RoutesEnabled && !(Form.ModifierKeys == Keys.Control))
         {
            DrawRoutes(e.Graphics);
         }

         if(MarkersEnabled && !(Form.ModifierKeys == Keys.Control))
         {
            foreach(GMapMarker m in Markers)
            {
               if(m.Visible && region.Contains(m.LocalPosition.X, m.LocalPosition.Y))
               {
                  m.OnRender(e.Graphics);
               }
            }
         }

         #region -- copyright --

         switch(Core.MapType)
         {
            case MapType.GoogleMap:
            case MapType.GoogleSatellite:
            case MapType.GoogleLabels:
            case MapType.GoogleTerrain:
            {
               e.Graphics.DrawString(Core.googleCopyright, gFont, Brushes.Navy, 3, Height - gFont.Height - 5);
            }
            break;

            case MapType.OpenStreetMap:
            case MapType.OpenStreetOsm:
            {
               e.Graphics.DrawString(Core.openStreetMapCopyright, gFont, Brushes.Navy, 3, Height - gFont.Height - 5);
            }
            break;

            case MapType.YahooMap:
            case MapType.YahooSatellite:
            case MapType.YahooLabels:
            {
               e.Graphics.DrawString(Core.yahooMapCopyright, gFont, Brushes.Navy, 3, Height - gFont.Height - 5);
            }
            break;
         }

         #endregion
      }

      protected override void OnResize(EventArgs e)
      {
         base.OnResize(e);

         if(this.Visible)
         {
            this.Invalidate(false);
         }
      }

      protected override void OnSizeChanged(EventArgs e)
      {
         Core.sizeOfMapArea = new GMapNET.Size(Width, Height);
         Core.sizeOfMapArea.Height /= GMaps.Instance.TileSize.Height;
         Core.sizeOfMapArea.Width /= GMaps.Instance.TileSize.Width;
         Core.sizeOfMapArea.Height += 1;
         Core.sizeOfMapArea.Width += 2;

         Core.sizeOfMapArea.Width = Core.sizeOfMapArea.Width/2 + 2;
         Core.sizeOfMapArea.Height = Core.sizeOfMapArea.Height/2 + 2;

         // 50px outside control
         region = new GMapNET.Rectangle(-50, -50, Size.Width+100, Size.Height+100);

         Core.OnMapSizeChanged(Width, Height);

         Core.GoToCurrentPosition();

         base.OnSizeChanged(e);
      }

      protected override void OnMouseDown(MouseEventArgs e)
      {
         Core.mouseDown.X = e.X;
         Core.mouseDown.Y = e.Y;

         if(e.Button == MouseButtons.Left)
         {
            if(!IsMouseOverMarker)
            {
               SetCurrentPositionOnly(e.X - Core.renderOffset.X, e.Y - Core.renderOffset.Y);

               if(Core.MouseVisible & !CurrentMarkerEnabled)
               {
                  this.Cursor = System.Windows.Forms.Cursors.Default;
                  Cursor.Hide();
                  Core.MouseVisible = false;
               }

               Core.BeginDrag(Core.mouseDown);
            }
         }
         else if(e.Button == MouseButtons.Right)
         {
            if(CanDragMap)
            {
               this.Cursor = System.Windows.Forms.Cursors.SizeAll;

               Core.BeginDrag(Core.mouseDown);
            }
         }

         this.Invalidate(false);

         base.OnMouseDown(e);
      }

      protected override void OnMouseUp(MouseEventArgs e)
      {
         if(Core.IsDragging)
         {
            Core.EndDrag();
         }

         this.Cursor = System.Windows.Forms.Cursors.Default;

         if(!Core.MouseVisible)
         {
            Cursor.Show();
            Core.MouseVisible = true;
         }

         base.OnMouseUp(e);
      }

      protected override void OnMouseClick(MouseEventArgs e)
      {
         if(e.Button == MouseButtons.Left)
         {
            //Core.CheckIfClickOnMarker(new GMapNET.Point(e.X, e.Y));
         }

         base.OnMouseClick(e);
      }

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
               SetCurrentPositionOnly(e.X - Core.renderOffset.X, e.Y - Core.renderOffset.Y);
               Invalidate(false);
            }
         }
         else
         {    
               foreach(GMapMarker m in Markers)
               {
                  if(m.Visible)
                  {
                     //Rectangle rc1 = Core.GetRectForMarker(e.Location, m);

                     //using(Region rg = new Region(rc1))
                     {
                        //if(rg.IsVisible(GMaps.Instance.FromLatLngToPixel(m.Position, Zoom)))
                        //{
                        //   this.Cursor = System.Windows.Forms.Cursors.Hand;
                        //   m.IsMouseOver = true;
                        //   //Core.isMouseOverMarker = true;
                        //   Invalidate(false);

                        //   if(Core.OnMarkerEnter != null)
                        //   {
                        //      Core.OnMarkerEnter(m);
                        //   }
                        //}
                        //else if(m.IsMouseOver)
                        //{
                        //   this.Cursor = System.Windows.Forms.Cursors.Default;
                        //   m.IsMouseOver = false;
                        //   //Core.isMouseOverMarker = false;
                        //   Invalidate(false);

                        //   if(Core.OnMarkerLeave != null)
                        //   {
                        //      Core.OnMarkerLeave(m);
                        //   }
                        //}
                     }
                  }
               }
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

      public bool SetCurrentPositionByKeywords(string keys)
      {
         return Core.SetCurrentPositionByKeywords(keys);
      }

      //public void SetCurrentMarkersVisibility(bool visible)
      //{
      //   Core.SetCurrentMarkersVisibility(visible);
      //}

      //public void SetCurrentMarkersTooltipMode(MarkerTooltipMode mode)
      //{
      //   Core.SetCurrentMarkersTooltipMode(mode);
      //}

      public PointLatLng FromLocalToLatLng(int x, int y)
      {
         return Core.FromLocalToLatLng(x, y);
      }

      public GMapNET.Point FromLatLngToLocal(PointLatLng point)
      {
         return Core.FromLatLngToLocal(point);
      }

      //public void AddRoute(Route item)
      //{
      //   Core.AddRoute(item);
      //}

      //public void RemoveRoute(Route item)
      //{
      //   Core.RemoveRoute(item);
      //}

      //public void ClearAllRoutes()
      //{
      //   Core.ClearAllRoutes();
      //}

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
         using(FileDialog dlg = new SaveFileDialog())
         {
            dlg.CheckPathExists = true;
            dlg.CheckFileExists = false;
            dlg.AddExtension = true;
            dlg.DefaultExt = "gmdb";
            dlg.ValidateNames = true;
            dlg.Title = "GMap.NET: Export map to db, if file exsist only new data will be added";
            dlg.FileName = "DataExp";
            dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            dlg.Filter = "GMap.NET DB files (*.gmdb)|*.gmdb";
            dlg.FilterIndex = 1;
            dlg.RestoreDirectory = true;

            if(dlg.ShowDialog() == DialogResult.OK)
            {
               bool ok = GMaps.Instance.ExportToGMDB(dlg.FileName);
               if(ok)
               {
                  MessageBox.Show("Complete!", "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Information);
               }
               else
               {
                  MessageBox.Show("  Failed!", "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Warning);
               }

               return ok;
            }
         }
         return false;
      }

      public bool ShowImportDialog()
      {
         using(FileDialog dlg = new OpenFileDialog())
         {
            dlg.CheckPathExists = true;
            dlg.CheckFileExists = false;
            dlg.AddExtension = true;
            dlg.DefaultExt = "gmdb";
            dlg.ValidateNames = true;
            dlg.Title = "GMap.NET: Import to db, only new data will be added";
            dlg.FileName = "DataExp";
            dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            dlg.Filter = "GMap.NET DB files (*.gmdb)|*.gmdb";
            dlg.FilterIndex = 1;
            dlg.RestoreDirectory = true;

            if(dlg.ShowDialog() == DialogResult.OK)
            {
               bool ok = GMaps.Instance.ImportFromGMDB(dlg.FileName);
               if(ok)
               {
                  MessageBox.Show("Complete!", "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Information);
               }
               else
               {
                  MessageBox.Show("  Failed!", "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Warning);
               }

               return ok;
            }
         }
         return false;
      }

      [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

      [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

      //public Font TooltipFont
      //{
      //   get
      //   {
      //      return Core.TooltipFont;
      //   }
      //   set
      //   {
      //      Core.TooltipFont = value;
      //   }
      //}

      public GMapNET.Size TooltipTextPadding
      {
         get
         {
            return Core.TooltipTextPadding;
         }
         set
         {
            Core.TooltipTextPadding = value;
         }
      }

      public MapType MapType
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

      //public CurrentMarkerType CurrentMarkerStyle
      //{
      //   get
      //   {
      //      return Core.CurrentMarkerStyle;
      //   }
      //   set
      //   {
      //      Core.CurrentMarkerStyle = value;
      //   }
      //}

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

      //public event MarkerClick OnMarkerClick
      //{
      //   add
      //   {
      //      Core.OnMarkerClick += value;
      //   }
      //   remove
      //   {
      //      Core.OnMarkerClick -= value;
      //   }
      //}

      //public event MarkerEnter OnMarkerEnter
      //{
      //   add
      //   {
      //      Core.OnMarkerEnter += value;
      //   }
      //   remove
      //   {
      //      Core.OnMarkerEnter -= value;
      //   }
      //}

      //public event MarkerLeave OnMarkerLeave
      //{
      //   add
      //   {
      //      Core.OnMarkerLeave += value;
      //   }
      //   remove
      //   {
      //      Core.OnMarkerLeave -= value;
      //   }
      //}

      public event MapDrag OnMapDrag
      {
         add
         {
            Core.OnMapDrag += value;
         }
         remove
         {
            Core.OnMapDrag -= value;
         }
      }

      public event MapZoomChanged OnMapZoomChanged
      {
         add
         {
            Core.OnMapZoomChanged += value;
         }
         remove
         {
            Core.OnMapZoomChanged -= value;
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
            Image m = Image.FromStream(stream, true, true);
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
