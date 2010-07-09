
namespace GMap.NET.WindowsForms
{
   using System;
   using System.ComponentModel;
   using System.Drawing;
   using System.Drawing.Drawing2D;
   using System.Drawing.Imaging;
   using System.IO;
   using System.Threading;
   using System.Windows.Forms;
   using GMap.NET;
   using GMap.NET.Internals;
   using GMap.NET.ObjectModel;

   /// <summary>
   /// GMap.NET control for Windows Forms
   /// </summary>   
   public partial class GMapControl : UserControl, IGControl
   {
      /// <summary>
      /// occurs when clicked on marker
      /// </summary>
      public event MarkerClick OnMarkerClick;

      /// <summary>
      /// occurs on mouse enters marker area
      /// </summary>
      public event MarkerEnter OnMarkerEnter;

      /// <summary>
      /// occurs on mouse leaves marker area
      /// </summary>
      public event MarkerLeave OnMarkerLeave;

      /// <summary>
      /// list of overlays, should be thread safe
      /// </summary>
      public readonly ObservableCollectionThreadSafe<GMapOverlay> Overlays = new ObservableCollectionThreadSafe<GMapOverlay>();

      /// <summary>
      /// max zoom
      /// </summary>         
      [Category("GMap.NET")]
      [Description("maximum zoom level of map")]
      public int MaxZoom
      {
         get
         {
            return Core.maxZoom;
         }
         set
         {
            Core.maxZoom = value;
         }
      }

      /// <summary>
      /// min zoom
      /// </summary>      
      [Category("GMap.NET")]
      [Description("minimum zoom level of map")]
      public int MinZoom
      {
         get
         {
            return Core.minZoom;
         }
         set
         {
            Core.minZoom = value;
         }
      }

      /// <summary>
      /// map zooming type for mouse wheel
      /// </summary>
      [Category("GMap.NET")]
      [Description("map zooming type for mouse wheel")]
      public MouseWheelZoomType MouseWheelZoomType
      {
         get
         {
            return Core.MouseWheelZoomType;
         }
         set
         {
            Core.MouseWheelZoomType = value;
         }
      }

      /// <summary>
      /// text on empty tiles
      /// </summary>
      public string EmptyTileText = "We are sorry, but we don't\nhave imagery at this zoom\nlevel for this region.";

      /// <summary>
      /// pen for empty tile borders
      /// </summary>
#if !PocketPC
      public Pen EmptyTileBorders = new Pen(Brushes.White, 1);
#else
      public Pen EmptyTileBorders = new Pen(Color.White, 1);
#endif

      /// <summary>
      /// pen for scale info
      /// </summary>
#if !PocketPC
      public Pen ScalePen = new Pen(Brushes.Blue, 1);
#else
      public Pen ScalePen = new Pen(Color.Blue, 1);
#endif

#if !PocketPC
      /// <summary>
      /// area selection pen
      /// </summary>
      public Pen SelectionPen = new Pen(Brushes.Blue, 2);

      /// <summary>
      /// background of selected area
      /// </summary>
      public Brush SelectedAreaFill = new SolidBrush(Color.FromArgb(33, Color.RoyalBlue));
#endif

      /// <summary>
      /// pen for empty tile background
      /// </summary>
#if !PocketPC
      public Brush EmptytileBrush = Brushes.Navy;
#else
      public Brush EmptytileBrush = new SolidBrush(Color.Navy);
#endif

#if PocketPC
      public Brush TileGridLinesTextBrush = new SolidBrush(Color.Red);
      public Brush TileGridMissingTextBrush = new SolidBrush(Color.White);
      public Brush CopyrightBrush = new SolidBrush(Color.Navy);
#endif

      /// <summary>
      /// show map scale info
      /// </summary>
      public bool MapScaleInfoEnabled = false;

      /// <summary>
      /// retry count to get tile 
      /// </summary>
      public int RetryLoadTile
      {
         get
         {
            return Core.RetryLoadTile;
         }
         set
         {
            Core.RetryLoadTile = value;
         }
      }

      /// <summary>
      /// how many levels of tiles are staying decompresed in memory
      /// </summary>
      public int LevelsKeepInMemmory
      {
         get
         {
            return Core.LevelsKeepInMemmory;
         }

         set
         {
            Core.LevelsKeepInMemmory = value;
         }
      }

      /// <summary>
      /// map dragg button
      /// </summary>
      public MouseButtons DragButton = MouseButtons.Right;

      private bool showTileGridLines = false;

      /// <summary>
      /// shows tile gridlines
      /// </summary>
      [Category("GMap.NET")]
      [Description("shows tile gridlines")]
      public bool ShowTileGridLines
      {
         get
         {
            return showTileGridLines;
         }
         set
         {
            showTileGridLines = value;
            Invalidate();
         }
      }

      /// <summary>
      /// current selected area in map
      /// </summary>
      private RectLatLng selectedArea;

      [Browsable(false)]
      public RectLatLng SelectedArea
      {
         get
         {
            return selectedArea;
         }
         set
         {
            selectedArea = value;

            if(Core.started)
            {
               Invalidate();
            }
         }
      }

      /// <summary>
      /// map boundaries
      /// </summary>
      public RectLatLng? BoundsOfMap = null;

      /// <summary>
      /// enables integrated DoubleBuffer for best performance
      /// if using a lot objets on map or running on windows mobile
      /// </summary>
#if !PocketPC
      public bool ForceDoubleBuffer = false;
#else
      readonly bool ForceDoubleBuffer = true;
#endif

      /// <summary>
      /// stops immediate marker/route/polygon invalidations;
      /// call Refresh to perform single refresh and reset invalidation state
      /// </summary>
      public bool HoldInvalidation = false;

      /// <summary>
      /// call this to stop HoldInvalidation and perform single refresh 
      /// </summary>
      public override void Refresh()
      {
         if(HoldInvalidation)
         {
            HoldInvalidation = false;
         }
         base.Refresh();
      }

#if !PocketPC
      private bool _GrayScale = false;

      [Category("GMap.NET")]
      public bool GrayScaleMode
      {
         get
         {
            return _GrayScale;
         }
         set
         {
            _GrayScale = value;
            if(GMaps.Instance.ImageProxy != null && GMaps.Instance.ImageProxy is WindowsFormsImageProxy)
            {
               (GMaps.Instance.ImageProxy as WindowsFormsImageProxy).GrayScale = value;
               this.ReloadMap();
            }
         }
      }
#endif

      // internal stuff
      internal readonly Core Core = new Core();
      internal readonly Font CopyrightFont = new Font(FontFamily.GenericSansSerif, 7, FontStyle.Regular);
#if !PocketPC
      internal readonly Font MissingDataFont = new Font(FontFamily.GenericSansSerif, 11, FontStyle.Bold);
#else
      internal readonly Font MissingDataFont = new Font(FontFamily.GenericSansSerif, 8, FontStyle.Regular);
#endif
      Font ScaleFont = new Font(FontFamily.GenericSansSerif, 5, FontStyle.Italic);
      internal readonly StringFormat CenterFormat = new StringFormat();
      internal readonly StringFormat BottomFormat = new StringFormat();

      double zoomReal;
      Bitmap backBuffer;
      Graphics gxOff;

#if !DESIGN
      /// <summary>
      /// construct
      /// </summary>
      public GMapControl()
      {
#if !PocketPC
         if(!DesignModeInConstruct && !IsDesignerHosted)
#endif
         {
            WindowsFormsImageProxy wimg = new WindowsFormsImageProxy();

#if !PocketPC
            wimg.GrayScale = this.GrayScaleMode;

            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.Opaque, true);
            ResizeRedraw = true;
#endif
            GMaps.Instance.ImageProxy = wimg;

            // to know when to invalidate
            Core.OnNeedInvalidation += new NeedInvalidation(Core_OnNeedInvalidation);
            Core.OnMapDrag += new MapDrag(Core_OnMapDrag);

            Core.SystemType = "WindowsForms";

            RenderMode = RenderMode.GDI_PLUS;
            Core.CurrentRegion = new GMap.NET.Rectangle(-50, -50, Size.Width+100, Size.Height+100);

            CenterFormat.Alignment = StringAlignment.Center;
            CenterFormat.LineAlignment = StringAlignment.Center;

            BottomFormat.Alignment = StringAlignment.Center;

#if !PocketPC
            BottomFormat.LineAlignment = StringAlignment.Far;
#endif
            MapType = MapType.GoogleMap;

            if(GMaps.Instance.IsRunningOnMono)
            {
               // no imports to move pointer
               MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionWithoutCenter;
            }
         }
      }
#endif

      /// <summary>
      /// update objects when map is draged
      /// </summary>
      internal void Core_OnMapDrag()
      {
         HoldInvalidation = true;

         foreach(GMapOverlay o in Overlays)
         {
            if(o.IsVisibile)
            {
               o.ForceUpdate();
            }
         }

         Refresh();
      }

      /// <summary>
      /// thread safe invalidation
      /// </summary>
      internal void Core_OnNeedInvalidation()
      {
         if(this.InvokeRequired)
         {
            MethodInvoker m = delegate
            {
#if !PocketPC
               Invalidate(false);
#else
               Invalidate();
#endif
            };
            try
            {
               this.Invoke(m);
            }
            catch
            {
            }
         }
         else
         {
#if !PocketPC
            Invalidate(false);
#else
            Invalidate();
#endif
         }
      }

      /// <summary>
      /// render map in GDI+
      /// </summary>
      /// <param name="g"></param>
      void DrawMapGDIplus(Graphics g)
      {
         Core.Matrix.EnterReadLock();
         Core.tileDrawingListLock.AcquireReaderLock();
         try
         {
            foreach(var tilePoint in Core.tileDrawingList)
            {
#if ContinuesMap
               //-----
               GMap.NET.Point tileToDraw = Core.tilePoint;  
               if(tileToDraw.X < Core.minOfTiles.Width)
               {
                  tileToDraw.X += (Core.maxOfTiles.Width + 1);
               }
               if(tileToDraw.X > Core.maxOfTiles.Width)
               {
                  tileToDraw.X -= (Core.maxOfTiles.Width + 1);
               }
               //-----
#endif
               {
                  Core.tileRect.X = tilePoint.X*Core.tileRect.Width;
                  Core.tileRect.Y = tilePoint.Y*Core.tileRect.Height;
                  Core.tileRect.Offset(Core.renderOffset);

                  if(Core.CurrentRegion.IntersectsWith(Core.tileRect))
                  {
                     bool found = false;
#if !ContinuesMap

                     Tile t = Core.Matrix.GetTileWithNoLock(Core.Zoom, tilePoint);
#else
                     Tile t = Core.Matrix.GetTileWithNoLock(Core.Zoom, tileToDraw);
#endif
                     if(t != null)
                     {
                        // render tile
                        lock(t.Overlays)
                        {
                           foreach(WindowsFormsImage img in t.Overlays)
                           {
                              if(img != null && img.Img != null)
                              {
                                 if(!found)
                                    found = true;
#if !PocketPC
                                 g.DrawImage(img.Img, Core.tileRect.X, Core.tileRect.Y, Core.tileRect.Width, Core.tileRect.Height);
#else
                                 g.DrawImage(img.Img, Core.tileRect.X, Core.tileRect.Y);
#endif
                              }
                           }
                        }
                     }

                     // add text if tile is missing
                     if(!found)
                     {
                        lock(Core.FailedLoads)
                        {
                           var lt = new LoadTask(tilePoint, Core.Zoom);
                           if(Core.FailedLoads.ContainsKey(lt))
                           {
                              var ex = Core.FailedLoads[lt];
#if !PocketPC
                              g.FillRectangle(EmptytileBrush, new RectangleF(Core.tileRect.X, Core.tileRect.Y, Core.tileRect.Width, Core.tileRect.Height));

                              g.DrawString("Exception: " + ex.Message, MissingDataFont, Brushes.Red, new RectangleF(Core.tileRect.X + 11, Core.tileRect.Y + 11, Core.tileRect.Width - 11, Core.tileRect.Height - 11));

                              g.DrawString(EmptyTileText, MissingDataFont, Brushes.Blue, new RectangleF(Core.tileRect.X, Core.tileRect.Y, Core.tileRect.Width, Core.tileRect.Height), CenterFormat);

#else
                              g.FillRectangle(EmptytileBrush, new System.Drawing.Rectangle(Core.tileRect.X, Core.tileRect.Y, Core.tileRect.Width, Core.tileRect.Height));

                              g.DrawString("Exception: " + ex.Message, MissingDataFont, TileGridMissingTextBrush, new RectangleF(Core.tileRect.X + 11, Core.tileRect.Y + 11, Core.tileRect.Width - 11, Core.tileRect.Height - 11));

                              g.DrawString(EmptyTileText, MissingDataFont, TileGridMissingTextBrush, new RectangleF(Core.tileRect.X, Core.tileRect.Y + Core.tileRect.Width/2 + (ShowTileGridLines? 11 : -22), Core.tileRect.Width, Core.tileRect.Height), BottomFormat);
#endif

                              g.DrawRectangle(EmptyTileBorders, Core.tileRect.X, Core.tileRect.Y, Core.tileRect.Width, Core.tileRect.Height);
                           }
                        }
                     }

                     if(ShowTileGridLines)
                     {
                        g.DrawRectangle(EmptyTileBorders, Core.tileRect.X, Core.tileRect.Y, Core.tileRect.Width, Core.tileRect.Height);
                        {
#if !PocketPC
                           g.DrawString((tilePoint == Core.centerTileXYLocation? "CENTER: " :"TILE: ") + tilePoint, MissingDataFont, Brushes.Red, new RectangleF(Core.tileRect.X, Core.tileRect.Y, Core.tileRect.Width, Core.tileRect.Height), CenterFormat);
#else
                           g.DrawString((tilePoint == Core.centerTileXYLocation? "" :"TILE: ") + tilePoint, MissingDataFont, TileGridLinesTextBrush, new RectangleF(Core.tileRect.X, Core.tileRect.Y, Core.tileRect.Width, Core.tileRect.Height), CenterFormat);
#endif
                        }
                     }
                  }
               }
            }
         }
         finally
         {
            Core.tileDrawingListLock.ReleaseReaderLock();
            Core.Matrix.LeaveReadLock();
         }
      }

      /// <summary>
      /// updates markers local position
      /// </summary>
      /// <param name="marker"></param>
      public void UpdateMarkerLocalPosition(GMapMarker marker)
      {
         GMap.NET.Point p = FromLatLngToLocal(marker.Position);
         marker.LocalPosition = new System.Drawing.Point(p.X + marker.Offset.X, p.Y  + marker.Offset.Y);
      }

      /// <summary>
      /// updates routes local position
      /// </summary>
      /// <param name="route"></param>
      public void UpdateRouteLocalPosition(GMapRoute route)
      {
         route.LocalPoints.Clear();

         foreach(GMap.NET.PointLatLng pg in route.Points)
         {
            GMap.NET.Point p = Projection.FromLatLngToPixel(pg, Core.Zoom);
            p.Offset(Core.renderOffset);
            route.LocalPoints.Add(p);
         }
      }

      /// <summary>
      /// updates polygons local position
      /// </summary>
      /// <param name="polygon"></param>
      public void UpdatePolygonLocalPosition(GMapPolygon polygon)
      {
         polygon.LocalPoints.Clear();

         foreach(GMap.NET.PointLatLng pg in polygon.Points)
         {
            GMap.NET.Point p = Projection.FromLatLngToPixel(pg, Core.Zoom);
            p.Offset(Core.renderOffset);
            polygon.LocalPoints.Add(p);
         }
      }

      /// <summary>
      /// sets zoom to max to fit rect
      /// </summary>
      /// <param name="rect"></param>
      /// <returns></returns>
      public bool SetZoomToFitRect(RectLatLng rect)
      {
         int maxZoom = Core.GetMaxZoomToFitRect(rect);
         if(maxZoom > 0)
         {
            PointLatLng center = new PointLatLng(rect.Lat-(rect.HeightLat/2), rect.Lng+(rect.WidthLng/2));
            CurrentPosition = center;

            if(maxZoom > MaxZoom)
            {
               maxZoom = MaxZoom;
            }

            if((int) Zoom != maxZoom)
            {
               Zoom = maxZoom;
            }

            return true;
         }
         return false;
      }

      /// <summary>
      /// sets to max zoom to fit all markers and centers them in map
      /// </summary>
      /// <param name="overlayId">overlay id or null to check all</param>
      /// <returns></returns>
      public bool ZoomAndCenterMarkers(string overlayId)
      {
         RectLatLng? rect = GetRectOfAllMarkers(overlayId);
         if(rect.HasValue)
         {
            return SetZoomToFitRect(rect.Value);
         }

         return false;
      }

      /// <summary>
      /// zooms and centers all route
      /// </summary>
      /// <param name="overlayId">overlay id or null to check all</param>
      /// <returns></returns>
      public bool ZoomAndCenterRoutes(string overlayId)
      {
         RectLatLng? rect = GetRectOfAllRoutes(overlayId);
         if(rect.HasValue)
         {
            return SetZoomToFitRect(rect.Value);
         }

         return false;
      }

      /// <summary>
      /// zooms and centers route 
      /// </summary>
      /// <param name="route"></param>
      /// <returns></returns>
      public bool ZoomAndCenterRoute(MapRoute route)
      {
         RectLatLng? rect = GetRectOfRoute(route);
         if(rect.HasValue)
         {
            return SetZoomToFitRect(rect.Value);
         }

         return false;
      }

      /// <summary>
      /// gets rectangle with all objects inside
      /// </summary>
      /// <param name="overlayId">overlay id or null to check all</param>
      /// <returns></returns>
      public RectLatLng? GetRectOfAllMarkers(string overlayId)
      {
         RectLatLng? ret = null;

         double left = double.MaxValue;
         double top = double.MinValue;
         double right = double.MinValue;
         double bottom = double.MaxValue;

         foreach(GMapOverlay o in Overlays)
         {
            if(overlayId == null || o.Id == overlayId)
            {
               if(o.IsVisibile && o.Markers.Count > 0)
               {
                  foreach(GMapMarker m in o.Markers)
                  {
                     if(m.IsVisible)
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
                  }
               }
            }
         }

         if(left != double.MaxValue && right != double.MinValue && top != double.MinValue && bottom != double.MaxValue)
         {
            ret = RectLatLng.FromLTRB(left, top, right, bottom);
         }

         return ret;
      }

      /// <summary>
      /// gets rectangle with all objects inside
      /// </summary>
      /// <param name="overlayId">overlay id or null to check all</param>
      /// <returns></returns>
      public RectLatLng? GetRectOfAllRoutes(string overlayId)
      {
         RectLatLng? ret = null;

         double left = double.MaxValue;
         double top = double.MinValue;
         double right = double.MinValue;
         double bottom = double.MaxValue;

         foreach(GMapOverlay o in Overlays)
         {
            if(overlayId == null || o.Id == overlayId)
            {
               if(o.IsVisibile && o.Routes.Count > 0)
               {
                  foreach(MapRoute route in o.Routes)
                  {
                     if(route.From.HasValue && route.To.HasValue)
                     {
                        foreach(PointLatLng p in route.Points)
                        {
                           // left
                           if(p.Lng < left)
                           {
                              left = p.Lng;
                           }

                           // top
                           if(p.Lat > top)
                           {
                              top = p.Lat;
                           }

                           // right
                           if(p.Lng > right)
                           {
                              right = p.Lng;
                           }

                           // bottom
                           if(p.Lat < bottom)
                           {
                              bottom = p.Lat;
                           }
                        }
                     }
                  }
               }
            }
         }

         if(left != double.MaxValue && right != double.MinValue && top != double.MinValue && bottom != double.MaxValue)
         {
            ret = RectLatLng.FromLTRB(left, top, right, bottom);
         }

         return ret;
      }

      /// <summary>
      /// gets rect of route
      /// </summary>
      /// <param name="route"></param>
      /// <returns></returns>
      public RectLatLng? GetRectOfRoute(MapRoute route)
      {
         RectLatLng? ret = null;

         double left = double.MaxValue;
         double top = double.MinValue;
         double right = double.MinValue;
         double bottom = double.MaxValue;

         if(route.From.HasValue && route.To.HasValue)
         {
            foreach(PointLatLng p in route.Points)
            {
               // left
               if(p.Lng < left)
               {
                  left = p.Lng;
               }

               // top
               if(p.Lat > top)
               {
                  top = p.Lat;
               }

               // right
               if(p.Lng > right)
               {
                  right = p.Lng;
               }

               // bottom
               if(p.Lat < bottom)
               {
                  bottom = p.Lat;
               }
            }
            ret = RectLatLng.FromLTRB(left, top, right, bottom);
         }
         return ret;
      }

#if !PocketPC
      /// <summary>
      /// gets image of the current view
      /// </summary>
      /// <returns></returns>
      public Image ToImage()
      {
         Image ret = null;
         try
         {
            using(Bitmap bitmap = new Bitmap(Width, Height))
            {
               using(Graphics g = Graphics.FromImage(bitmap))
               {
                  using(Graphics gg = this.CreateGraphics())
                  {
#if !PocketPC
                     g.CopyFromScreen(PointToScreen(new System.Drawing.Point()).X, PointToScreen(new System.Drawing.Point()).Y, 0, 0, new System.Drawing.Size(Width, Height));
#else
                     throw new NotImplementedException("Not implemeted for PocketPC");
#endif
                  }
               }

               // Convert the Image to a png
               using(MemoryStream ms = new MemoryStream())
               {
                  bitmap.Save(ms, ImageFormat.Png);
#if !PocketPC
                  ret = Image.FromStream(ms);
#else
                  throw new NotImplementedException("Not implemeted for PocketPC");
#endif
               }
            }
         }
         catch
         {
            ret = null;
         }
         return ret;
      }
#endif

      /// <summary>
      /// offset position in pixels
      /// </summary>
      /// <param name="x"></param>
      /// <param name="y"></param>
      public void Offset(int x, int y)
      {
         if(IsHandleCreated)
         {
            Core.DragOffset(new GMap.NET.Point(x, y));
         }
      }

      #region UserControl Events

#if !PocketPC
      protected bool DesignModeInConstruct
      {
         get
         {
            return (LicenseManager.UsageMode == LicenseUsageMode.Designtime);
         }
      }

      public bool IsDesignerHosted
      {
         get
         {
            return IsControlDesignerHosted(this);
         }
      }

      public bool IsControlDesignerHosted(Control ctrl)
      {
         if(ctrl != null)
         {
            if(ctrl.Site != null)
            {

               if(ctrl.Site.DesignMode == true)
                  return true;

               else
               {
                  if(IsControlDesignerHosted(ctrl.Parent))
                     return true;

                  else
                     return false;
               }
            }
            else
            {
               if(IsControlDesignerHosted(ctrl.Parent))
                  return true;
               else
                  return false;
            }
         }
         else
            return false;
      }

      protected override void OnLoad(EventArgs e)
      {
         base.OnLoad(e);

         if(!IsDesignerHosted)
         {
            MethodInvoker m = delegate
            {
               Thread.Sleep(222);
               Core.StartSystem();
            };
            this.BeginInvoke(m);
         }
      }
#else
      delegate void MethodInvoker();
      bool IsHandleCreated = false;

      protected override void OnHandleCreated(EventArgs e)
      {
         base.OnHandleCreated(e);
         {
            MethodInvoker m = delegate
            {
               Thread.Sleep(222);
               Core.StartSystem();
            };
            this.BeginInvoke(m);
         }
         IsHandleCreated = true;
      }

      protected override void OnPaintBackground(PaintEventArgs e)
      {
         // ...
      }
#endif

      protected override void OnHandleDestroyed(EventArgs e)
      {
         Core.OnMapClose();
         Core.ApplicationExit();

         base.OnHandleDestroyed(e);
      }

      PointLatLng selectionStart;
      PointLatLng selectionEnd;

#if !PocketPC
      float? MapRenderTransform = null;
#endif

      protected override void OnPaint(PaintEventArgs e)
      {
         if(ForceDoubleBuffer)
         {
            if(gxOff != null && backBuffer != null)
            {
               // render white background
               gxOff.Clear(Color.WhiteSmoke);

#if !PocketPC
               if(MapRenderTransform.HasValue)
               {
                  gxOff.ScaleTransform(MapRenderTransform.Value, MapRenderTransform.Value);
                  {
                     DrawMapGDIplus(gxOff);
                  }
                  gxOff.ResetTransform();
               }
               else
#endif
               {
                  DrawMapGDIplus(gxOff);
               }

               OnPaintEtc(gxOff);

               e.Graphics.DrawImage(backBuffer, 0, 0);
            }
         }
         else
         {
            e.Graphics.Clear(Color.WhiteSmoke);

#if !PocketPC
            if(MapRenderTransform.HasValue)
            {
               e.Graphics.ScaleTransform(MapRenderTransform.Value, MapRenderTransform.Value);
               {
                  DrawMapGDIplus(e.Graphics);
               }
               e.Graphics.ResetTransform();
            }
            else
#endif
            {
               DrawMapGDIplus(e.Graphics);
            }
            OnPaintEtc(e.Graphics);
         }

         base.OnPaint(e);
      }

      /// <summary>
      /// override, to render something more
      /// </summary>
      /// <param name="g"></param>
      protected virtual void OnPaintEtc(Graphics g)
      {
#if !PocketPC
         g.SmoothingMode = SmoothingMode.HighQuality;
#endif
         foreach(GMapOverlay o in Overlays)
         {
            if(o.IsVisibile)
            {
               o.Render(g);
            }
         }

#if !PocketPC
         if(!SelectedArea.IsEmpty)
         {
            GMap.NET.Point p1 = FromLatLngToLocal(SelectedArea.LocationTopLeft);
            GMap.NET.Point p2 = FromLatLngToLocal(SelectedArea.LocationRightBottom);

            int x1 = p1.X;
            int y1 = p1.Y;
            int x2 = p2.X;
            int y2 = p2.Y;

            g.DrawRectangle(SelectionPen, x1, y1, x2 - x1, y2 - y1);
            g.FillRectangle(SelectedAreaFill, x1, y1, x2 - x1, y2 - y1);
         }
#endif

         #region -- copyright --

         switch(Core.MapType)
         {
            case MapType.GoogleMap:
            case MapType.GoogleSatellite:
            case MapType.GoogleLabels:
            case MapType.GoogleTerrain:
            case MapType.GoogleHybrid:
            {
#if !PocketPC
               g.DrawString(Core.googleCopyright, CopyrightFont, Brushes.Navy, 3, Height - CopyrightFont.Height - 5);
#else
               g.DrawString(Core.googleCopyright, CopyrightFont, CopyrightBrush, 3, Height - CopyrightFont.Size - 15);
#endif
            }
            break;

            case MapType.OpenStreetMap:
            case MapType.OpenStreetOsm:
            case MapType.OpenStreetMapSurfer:
            case MapType.OpenStreetMapSurferTerrain:
            {
#if !PocketPC
               g.DrawString(Core.openStreetMapCopyright, CopyrightFont, Brushes.Navy, 3, Height - CopyrightFont.Height - 5);
#else
               g.DrawString(Core.openStreetMapCopyright, CopyrightFont, CopyrightBrush, 3, Height - CopyrightFont.Size - 15);
#endif
            }
            break;

            case MapType.YahooMap:
            case MapType.YahooSatellite:
            case MapType.YahooLabels:
            case MapType.YahooHybrid:
            {
#if !PocketPC
               g.DrawString(Core.yahooMapCopyright, CopyrightFont, Brushes.Navy, 3, Height - CopyrightFont.Height - 5);
#else
               g.DrawString(Core.yahooMapCopyright, CopyrightFont, CopyrightBrush, 3, Height - CopyrightFont.Size - 15);
#endif
            }
            break;

            case MapType.BingHybrid:
            case MapType.BingMap:
            case MapType.BingSatellite:
            {
#if !PocketPC
               g.DrawString(Core.virtualEarthCopyright, CopyrightFont, Brushes.Navy, 3, Height - CopyrightFont.Height - 5);
#else
               g.DrawString(Core.virtualEarthCopyright, CopyrightFont, CopyrightBrush, 3, Height - CopyrightFont.Size - 15);
#endif
            }
            break;

            case MapType.ArcGIS_Map:
            case MapType.ArcGIS_Satellite:
            case MapType.ArcGIS_ShadedRelief:
            case MapType.ArcGIS_Terrain:
            {
#if !PocketPC
               g.DrawString(Core.arcGisCopyright, CopyrightFont, Brushes.Navy, 3, Height - CopyrightFont.Height - 5);
#else
               g.DrawString(Core.arcGisCopyright, CopyrightFont, CopyrightBrush, 3, Height - CopyrightFont.Size - 15);
#endif
            }
            break;

            case MapType.MapsLT_OrtoFoto:
            case MapType.MapsLT_Map:
            case MapType.MapsLT_Map_Hybrid:
            case MapType.MapsLT_Map_Labels:
            {
#if !PocketPC
               g.DrawString(Core.hnitCopyright, CopyrightFont, Brushes.Navy, 3, Height - CopyrightFont.Height - 5);
#else
               g.DrawString(Core.hnitCopyright, CopyrightFont, CopyrightBrush, 3, Height - CopyrightFont.Size - 15);
#endif
            }
            break;

            case MapType.PergoTurkeyMap:
            {
#if !PocketPC
               g.DrawString(Core.pergoCopyright, CopyrightFont, Brushes.Navy, 3, Height - CopyrightFont.Height - 5);
#else
               g.DrawString(Core.pergoCopyright, CopyrightFont, CopyrightBrush, 3, Height - CopyrightFont.Size - 15);
#endif
            }
            break;
         }

         #endregion

         #region -- draw scale --
#if !PocketPC
         if(MapScaleInfoEnabled)
         {
            if(Width > Core.pxRes5000km)
            {
               g.DrawRectangle(ScalePen, 10, 10, Core.pxRes5000km, 10);
               g.DrawString("5000Km", ScaleFont, Brushes.Blue, Core.pxRes5000km + 10, 11);
            }
            if(Width > Core.pxRes1000km)
            {
               g.DrawRectangle(ScalePen, 10, 10, Core.pxRes1000km, 10);
               g.DrawString("1000Km", ScaleFont, Brushes.Blue, Core.pxRes1000km + 10, 11);
            }
            if(Width > Core.pxRes100km && Zoom > 2)
            {
               g.DrawRectangle(ScalePen, 10, 10, Core.pxRes100km, 10);
               g.DrawString("100Km", ScaleFont, Brushes.Blue, Core.pxRes100km + 10, 11);
            }
            if(Width > Core.pxRes10km && Zoom > 5)
            {
               g.DrawRectangle(ScalePen, 10, 10, Core.pxRes10km, 10);
               g.DrawString("10Km", ScaleFont, Brushes.Blue, Core.pxRes10km + 10, 11);
            }
            if(Width > Core.pxRes1000m && Zoom >= 10)
            {
               g.DrawRectangle(ScalePen, 10, 10, Core.pxRes1000m, 10);
               g.DrawString("1000m", ScaleFont, Brushes.Blue, Core.pxRes1000m + 10, 11);
            }
            if(Width > Core.pxRes100m && Zoom > 11)
            {
               g.DrawRectangle(ScalePen, 10, 10, Core.pxRes100m, 10);
               g.DrawString("100m", ScaleFont, Brushes.Blue, Core.pxRes100m + 9, 11);
            }
         }
#endif
         #endregion
      }

#if !PocketPC
      protected override void OnSizeChanged(EventArgs e)
      {
         base.OnSizeChanged(e);

         if(!IsDesignerHosted && !DesignModeInConstruct)
         {
            if(ForceDoubleBuffer)
            {
               if(backBuffer != null)
               {
                  backBuffer.Dispose();
                  backBuffer = null;
               }
               if(gxOff != null)
               {
                  gxOff.Dispose();
                  gxOff = null;
               }

               backBuffer = new Bitmap(Width, Height);
               gxOff = Graphics.FromImage(backBuffer);
            }

            Core.OnMapSizeChanged(Width, Height);

            // 50px outside control
            Core.CurrentRegion = new GMap.NET.Rectangle(-50, -50, Size.Width+100, Size.Height+100);

            if(Visible && IsHandleCreated)
            {
               // keep center on same position
               Core.GoToCurrentPosition();
            }
         }
      }
#else
#if !DESIGN
      protected override void OnResize(EventArgs e)
      {
         base.OnResize(e);

         if(ForceDoubleBuffer)
         {
            if(backBuffer != null)
            {
               backBuffer.Dispose();
               backBuffer = null;
            }
            if(gxOff != null)
            {
               gxOff.Dispose();
               gxOff = null;
            }

            backBuffer = new Bitmap(Width, Height);
            gxOff = Graphics.FromImage(backBuffer);
         }

         Core.OnMapSizeChanged(Width, Height);

         // 50px outside control
         Core.CurrentRegion = new GMap.NET.Rectangle(-50, -50, Size.Width+100, Size.Height+100);

         if(Core.started)
         {
            if(Visible) // && IsHandleCreated
            {
               // keep center on same position
               Core.GoToCurrentPosition();
            }
         }
      }
#endif
#endif

      bool isSelected = false;
      protected override void OnMouseDown(MouseEventArgs e)
      {
         if(!IsMouseOverMarker)
         {
#if !PocketPC
            if(e.Button == DragButton && CanDragMap)
#else
            if(CanDragMap)
#endif
            {
               Core.mouseDown.X = e.X;
               Core.mouseDown.Y = e.Y;

#if !PocketPC
               this.Cursor = System.Windows.Forms.Cursors.SizeAll;
#endif
               Core.BeginDrag(Core.mouseDown);

#if !PocketPC
               this.Invalidate(false);
#else
               this.Invalidate();
#endif
            }
            else if(!isSelected)
            {
               isSelected = true;
               SelectedArea = RectLatLng.Empty;
               selectionEnd = PointLatLng.Empty;
               selectionStart = FromLocalToLatLng(e.X, e.Y);
            }
         }

         base.OnMouseDown(e);
      }

      protected override void OnMouseUp(MouseEventArgs e)
      {
         base.OnMouseUp(e);

         if(isSelected)
         {
            isSelected = false;
         }

         if(Core.IsDragging)
         {
            Core.EndDrag();

#if !PocketPC
            this.Cursor = System.Windows.Forms.Cursors.Default;
#endif
            if(BoundsOfMap.HasValue && !BoundsOfMap.Value.Contains(CurrentPosition))
            {
               if(Core.LastLocationInBounds.HasValue)
               {
                  CurrentPosition = Core.LastLocationInBounds.Value;
               }
            }
         }
         else
         {
#if !PocketPC
            if(!selectionEnd.IsEmpty && !selectionStart.IsEmpty)
            {
               if(!SelectedArea.IsEmpty && Form.ModifierKeys == Keys.Shift)
               {
                  SetZoomToFitRect(SelectedArea);
               }
            }
#endif
         }
      }

#if !PocketPC
      protected override void OnMouseClick(MouseEventArgs e)
      {
         if(!Core.IsDragging)
         {
            for(int i = Overlays.Count-1; i >= 0; i--)
            {
               GMapOverlay o = Overlays[i];
               if(o != null && o.IsVisibile)
               {
                  foreach(GMapMarker m in o.Markers)
                  {
                     if(m.IsVisible && m.IsHitTestVisible)
                     {
                        if(m.LocalArea.Contains(e.X, e.Y))
                        {
                           if(OnMarkerClick != null)
                           {
                              OnMarkerClick(m, e);
                              break;
                           }
                        }
                     }
                  }
               }
            }
         }

         base.OnMouseClick(e);
      }
#endif

      protected override void OnMouseMove(MouseEventArgs e)
      {
         if(Core.IsDragging)
         {
            if(BoundsOfMap.HasValue && !BoundsOfMap.Value.Contains(CurrentPosition))
            {
               // ...
            }
            else
            {
               Core.mouseCurrent.X = e.X;
               Core.mouseCurrent.Y = e.Y;
               {
                  Core.Drag(Core.mouseCurrent);
               }
            }
         }
         else
         {
#if !PocketPC
            if(isSelected && !selectionStart.IsEmpty && (Form.ModifierKeys == Keys.Alt || Form.ModifierKeys == Keys.Shift))
            {
               selectionEnd = FromLocalToLatLng(e.X, e.Y);
               {
                  GMap.NET.PointLatLng p1 = selectionStart;
                  GMap.NET.PointLatLng p2 = selectionEnd;

                  double x1 = Math.Min(p1.Lng, p2.Lng);
                  double y1 = Math.Max(p1.Lat, p2.Lat);
                  double x2 = Math.Max(p1.Lng, p2.Lng);
                  double y2 = Math.Min(p1.Lat, p2.Lat);

                  SelectedArea = new RectLatLng(y1, x1, x2 - x1, y1 - y2);
               }
            }
            else
#endif
            {
               for(int i = Overlays.Count-1; i >= 0; i--)
               {
                  GMapOverlay o = Overlays[i];
                  if(o != null && o.IsVisibile)
                  {
                     foreach(GMapMarker m in o.Markers)
                     {
                        if(m.IsVisible && m.IsHitTestVisible)
                        {
                           if(m.LocalArea.Contains(e.X, e.Y))
                           {
#if !PocketPC
                              this.Cursor = System.Windows.Forms.Cursors.Hand;
#endif
                              m.IsMouseOver = true;
#if !PocketPC
                              Invalidate(false);
#else
                              Invalidate();
#endif

                              if(OnMarkerEnter != null)
                              {
                                 OnMarkerEnter(m);
                              }
                           }
                           else if(m.IsMouseOver)
                           {
#if !PocketPC
                              this.Cursor = System.Windows.Forms.Cursors.Default;
#endif
                              m.IsMouseOver = false;
#if !PocketPC
                              Invalidate(false);
#else
                              Invalidate();
#endif

                              if(OnMarkerLeave != null)
                              {
                                 OnMarkerLeave(m);
                              }
                           }
                        }
                     }
                  }
               }
            }
         }
         base.OnMouseMove(e);
      }

#if !PocketPC

      protected override void OnMouseWheel(MouseEventArgs e)
      {
         base.OnMouseWheel(e);

         if(!IsMouseOverMarker && !IsDragging)
         {
            if(Core.mouseLastZoom.X != e.X && Core.mouseLastZoom.Y != e.Y)
            {
               if(MouseWheelZoomType == MouseWheelZoomType.MousePositionAndCenter)
               {
                  Core.currentPosition = FromLocalToLatLng(e.X, e.Y);
               }
               else if(MouseWheelZoomType == MouseWheelZoomType.ViewCenter)
               {
                  Core.currentPosition = FromLocalToLatLng((int) Width/2, (int) Height/2);
               }
               else if(MouseWheelZoomType == MouseWheelZoomType.MousePositionWithoutCenter)
               {
                  Core.currentPosition = FromLocalToLatLng(e.X, e.Y);
               }

               Core.mouseLastZoom.X = e.X;
               Core.mouseLastZoom.Y = e.Y;
            }

            // set mouse position to map center
            if(MouseWheelZoomType != MouseWheelZoomType.MousePositionWithoutCenter)
            {
               if(!GMaps.Instance.IsRunningOnMono)
               {
                  System.Drawing.Point p = PointToScreen(new System.Drawing.Point(Width/2, Height/2));
                  Stuff.SetCursorPos((int) p.X, (int) p.Y);
               }
            }

            Core.MouseWheelZooming = true;

            if(e.Delta > 0)
            {
               Zoom++;
            }
            else if(e.Delta < 0)
            {
               Zoom--;
            }

            Core.MouseWheelZooming = false;
         }
      }
#endif
      #endregion

      #region IGControl Members

      /// <summary>
      /// Call it to empty tile cache & reload tiles
      /// </summary>
      public void ReloadMap()
      {
         Core.ReloadMap();
      }

      /// <summary>
      /// set current position using keywords
      /// </summary>
      /// <param name="keys"></param>
      /// <returns>true if successfull</returns>
      public GeoCoderStatusCode SetCurrentPositionByKeywords(string keys)
      {
         GeoCoderStatusCode status = GeoCoderStatusCode.Unknow;
         PointLatLng? pos = Manager.GetLatLngFromGeocoder(keys, out status);
         if(pos.HasValue && status == GeoCoderStatusCode.G_GEO_SUCCESS)
         {
            CurrentPosition = pos.Value;
         }

         return status;
      }

      /// <summary>
      /// gets world coordinate from local control coordinate 
      /// </summary>
      /// <param name="x"></param>
      /// <param name="y"></param>
      /// <returns></returns>
      public PointLatLng FromLocalToLatLng(int x, int y)
      {
#if !PocketPC
         if(MapRenderTransform.HasValue)
         {
            // var tp = MapRenderTransform.Inverse.Transform(new System.Windows.Point(x, y));
            //x = (int) tp.X;
            //y = (int) tp.Y;
            x = (int) (x * MapRenderTransform.Value);
            y = (int) (y * MapRenderTransform.Value);
         }
#endif
         return Core.FromLocalToLatLng(x, y);
      }

      /// <summary>
      /// gets local coordinate from world coordinate
      /// </summary>
      /// <param name="point"></param>
      /// <returns></returns>
      public GMap.NET.Point FromLatLngToLocal(PointLatLng point)
      {
         GMap.NET.Point ret = Core.FromLatLngToLocal(point);

#if !PocketPC
         if(MapRenderTransform.HasValue)
         {
            //var tp = MapRenderTransform.Transform(new System.Windows.Point(ret.X, ret.Y));
            ret.X = (int) (ret.X / MapRenderTransform.Value);
            ret.Y = (int) (ret.X / MapRenderTransform.Value);
         }
#endif
         return ret;
      }

#if !PocketPC

      /// <summary>
      /// shows map db export dialog
      /// </summary>
      /// <returns></returns>
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
                  MessageBox.Show("Failed!", "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Warning);
               }

               return ok;
            }
         }

         return false;
      }

      /// <summary>
      /// shows map dbimport dialog
      /// </summary>
      /// <returns></returns>
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
                  MessageBox.Show("Failed!", "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Warning);
               }

               return ok;
            }
         }

         return false;
      }
#endif

      [Category("GMap.NET")]
      public double Zoom
      {
         get
         {
            return zoomReal;
         }
         set
         {
            if(zoomReal != value)
            {
               if(value > MaxZoom)
               {
                  zoomReal = MaxZoom;
               }
               else
                  if(value < MinZoom)
                  {
                     zoomReal = MinZoom;
                  }
                  else
                  {
                     zoomReal = value;
                  }

               float remainder = (float) System.Decimal.Remainder((Decimal) value, (Decimal) 1);
               if(remainder != 0)
               {
                  float scaleValue = remainder + 1;
                  {
#if !PocketPC
                     MapRenderTransform = scaleValue;
#endif
                  }

                  ZoomStep = Convert.ToInt32(value - remainder);
#if !PocketPC
                  Invalidate(false);
#else
                  Invalidate();
#endif
               }
               else
               {
#if !PocketPC
                  MapRenderTransform = null;
#endif
                  ZoomStep = Convert.ToInt32(value);
                  zoomReal = ZoomStep;
#if !PocketPC
                  Invalidate(false);
#else
                  Invalidate();
#endif
               }
            }
         }
      }

      /// <summary>
      /// map zoom level
      /// </summary>
      [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
      [Browsable(false)]
      internal int ZoomStep
      {
         get
         {
            return Core.Zoom;
         }
         set
         {
            if(value > MaxZoom)
            {
               Core.Zoom = MaxZoom;
            }
            else if(value < MinZoom)
            {
               Core.Zoom = MinZoom;
            }
            else
            {
               Core.Zoom = value;
            }
         }
      }

      /// <summary>
      /// current map center position
      /// </summary>
      [Browsable(false)]
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

      /// <summary>
      /// current marker position in pixel coordinates
      /// </summary>
      [Browsable(false)]
      public GMap.NET.Point CurrentPositionGPixel
      {
         get
         {
            return Core.CurrentPositionGPixel;
         }
      }

      /// <summary>
      /// location of cache
      /// </summary>
      [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
      [Browsable(false)]
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

      /// <summary>
      /// is user dragging map
      /// </summary>
      [Browsable(false)]
      public bool IsDragging
      {
         get
         {
            return Core.IsDragging;
         }
      }

      bool isMouseOverMarker;

      /// <summary>
      /// is mouse over marker
      /// </summary>
      public bool IsMouseOverMarker
      {
         get
         {
            return isMouseOverMarker;
         }
         internal set
         {
            isMouseOverMarker = value;
         }
      }

      /// <summary>
      /// gets current map view top/left coordinate, width in Lng, height in Lat
      /// </summary>
      [Browsable(false)]
      public RectLatLng CurrentViewArea
      {
         get
         {
            return Core.CurrentViewArea;
         }
      }

      /// <summary>
      /// type of map
      /// </summary>
      [Category("GMap.NET")]
      public MapType MapType
      {
         get
         {
            return Core.MapType;
         }
         set
         {
            if(Core.MapType != value)
            {
               RectLatLng viewarea = SelectedArea;
               if(viewarea != RectLatLng.Empty)
               {
                  CurrentPosition = new PointLatLng(viewarea.Lat - viewarea.HeightLat/2, viewarea.Lng + viewarea.WidthLng/2);
               }
               else
               {
                  viewarea = CurrentViewArea;
               }

               Core.MapType = value;

               if(Core.started && Core.zoomToArea)
               {
                  // restore zoomrect as close as possible
                  if(viewarea != RectLatLng.Empty && viewarea != CurrentViewArea)
                  {
                     int bestZoom = Core.GetMaxZoomToFitRect(viewarea);
                     if(bestZoom > 0 && Zoom != bestZoom)
                     {
                        Zoom = bestZoom;
                     }
                  }
               }
            }
         }
      }

      /// <summary>
      /// map projection
      /// </summary>
      [Browsable(false)]
      public PureProjection Projection
      {
         get
         {
            return Core.Projection;
         }
      }

      /// <summary>
      /// is routes enabled
      /// </summary>
      [Category("GMap.NET")]
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

      /// <summary>
      /// is polygons enabled
      /// </summary>
      [Category("GMap.NET")]
      public bool PolygonsEnabled
      {
         get
         {
            return Core.PolygonsEnabled;
         }
         set
         {
            Core.PolygonsEnabled = value;
         }
      }

      /// <summary>
      /// is markers enabled
      /// </summary>
      [Category("GMap.NET")]
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

      /// <summary>
      /// can user drag map
      /// </summary>
      [Category("GMap.NET")]
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

      /// <summary>
      /// map render mode
      /// </summary>
      [Browsable(false)]
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

      /// <summary>
      /// gets map manager
      /// </summary>
      public GMaps Manager
      {
         get
         {
            return GMaps.Instance;
         }
      }

      #endregion

      #region IGControl event Members

      /// <summary>
      /// occurs when current position is changed
      /// </summary>
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

      /// <summary>
      /// occurs when tile set load is complete
      /// </summary>
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

      /// <summary>
      /// occurs when tile set is starting to load
      /// </summary>
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

      /// <summary>
      /// occurs on map drag
      /// </summary>
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

      /// <summary>
      /// occurs on map zoom changed
      /// </summary>
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

      /// <summary>
      /// occures on map type changed
      /// </summary>
      public event MapTypeChanged OnMapTypeChanged
      {
         add
         {
            Core.OnMapTypeChanged += value;
         }
         remove
         {
            Core.OnMapTypeChanged -= value;
         }
      }

      /// <summary>
      /// occurs on empty tile displayed
      /// </summary>
      public event EmptyTileError OnEmptyTileError
      {
         add
         {
            Core.OnEmptyTileError += value;
         }
         remove
         {
            Core.OnEmptyTileError -= value;
         }
      }

      #endregion
   }
}
