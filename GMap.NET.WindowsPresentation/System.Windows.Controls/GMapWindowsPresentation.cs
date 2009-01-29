using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Drawing;
using System.IO;

using GMapNET;
using GMapNET.Properties;
using GMapNET.Internals;

namespace System.Windows.Controls
{
   public partial class GMap : UserControl, IGMapControl
   {
      readonly Core Core = new Core();
      delegate void MethodInvoker();
      Region Region = new Region();
      System.Windows.Media.Pen pen = new System.Windows.Media.Pen(System.Windows.Media.Brushes.Red, 2);

      public GMap()
      {
         Purity.Instance.ImageProxy = new WindowsPresentationImageProxy();

         Core.RenderMode = GMapNET.RenderMode.WPF;
         Core.OnNeedInvalidation += new NeedInvalidation(Core_OnNeedInvalidation);

         SnapsToDevicePixels = true;
         ClipToBounds = true;
         SizeChanged += new SizeChangedEventHandler(GMap_SizeChanged);
         Loaded += new RoutedEventHandler(GMap_Loaded);
      }

      /// <summary>
      /// inits core system
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void GMap_Loaded(object sender, RoutedEventArgs e)
      {
         Core.StartSystem();
      }

      /// <summary>
      /// on core needs invalidation
      /// </summary>
      void Core_OnNeedInvalidation()
      {
         if(this.Dispatcher.CheckAccess())
         {
            this.InvalidateVisual();
         }
         else
         {
            MethodInvoker m = delegate
            {
               this.InvalidateVisual();
            };
            this.Dispatcher.Invoke(DispatcherPriority.Render, m);
         }
      }

      /// <summary>
      /// on map size changed
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void GMap_SizeChanged(object sender, SizeChangedEventArgs e)
      {
         Core.sizeOfMapArea = new System.Drawing.Size((int) e.NewSize.Width, (int) e.NewSize.Height);
         Core.sizeOfMapArea.Height /= GMaps.Instance.TileSize.Height;
         Core.sizeOfMapArea.Width /= GMaps.Instance.TileSize.Width;
         Core.sizeOfMapArea.Height += 1;
         Core.sizeOfMapArea.Width += 1;

         Core.sizeOfMapArea.Width = Core.sizeOfMapArea.Width/2 + 2;
         Core.sizeOfMapArea.Height = Core.sizeOfMapArea.Height/2 + 2;

         // 50px outside control
         this.Region = new Region(new System.Drawing.Rectangle(-50, -50, (int) e.NewSize.Width+100, (int) e.NewSize.Height+100));

         Core.OnMapSizeChanged((int) e.NewSize.Width, (int) e.NewSize.Height);
         InvalidateVisual();
      }

      /// <summary>
      /// render map in WPF
      /// </summary>
      /// <param name="g"></param>
      void DrawMapWPF(DrawingContext g)
      {
         for(int i = -(Core.sizeOfMapArea.Width + Core.centerTileXYOffset.X); i < (Core.sizeOfMapArea.Width - Core.centerTileXYOffset.X); i++)
         {
            for(int j = -(Core.sizeOfMapArea.Height + Core.centerTileXYOffset.Y); j < (Core.sizeOfMapArea.Height - Core.centerTileXYOffset.Y); j++)
            {
               Core.tilePoint = CurrentPositionGTile;
               Core.tilePoint.X += i;
               Core.tilePoint.Y += j;

               Tile t = Core.Matrix[Core.tilePoint];
               if(t != null) // debug center tile add: && p != centerTileXYLocation
               {
                  if(t.Image != null)
                  {
                     Core.tileRect.Location = new System.Drawing.Point(Core.tilePoint.X*Core.tileRect.Width, Core.tilePoint.Y*Core.tileRect.Height);
                     Core.tileRect.Offset(Core.renderOffset);

                     if(this.Region.IsVisible(Core.tileRect))
                     {
                        WindowsPresentationImage img = t.Image as WindowsPresentationImage;
                        if(img != null)
                        {
                           g.DrawImage(img.Img, new Rect(Core.tileRect.X, Core.tileRect.Y, 256, 256));
                        }
                     }
                  }
               }
            }
         }
      }

      #region UserControl Events
      protected override void OnRender(DrawingContext drawingContext)
      {
         if(Core.RenderMode == GMapNET.RenderMode.WPF)
         {
            DrawMapWPF(drawingContext);
         }
      }

      protected override void OnMouseDown(MouseButtonEventArgs e)
      {
         Core.mouseDown = new System.Drawing.Point((int) e.GetPosition(this).X, (int) e.GetPosition(this).Y);

         if(e.LeftButton == MouseButtonState.Pressed)
         {
            if(CurrentMarkerEnabled && !IsMouseOverMarker)
            {
               ChangeCurrentPositionOnly(new System.Drawing.Point(Core.mouseDown.X - Core.renderOffset.X, Core.mouseDown.Y - Core.renderOffset.Y));

               if(Core.MouseVisible)
               {
                  //this.Cursor = System.Windows.Forms.Cursors.Default;
                  //this.Cursor.Hide();
                  Core.MouseVisible = false;
               }

               Core.BeginDrag(Core.mouseDown);
            }
         }
         else if(e.RightButton == MouseButtonState.Pressed)
         {
            if(CanDragMap)
            {
               //this.Cursor = System.Windows.Forms.Cursors.SizeAll;

               Core.BeginDrag(Core.mouseDown);
            }
         }

         InvalidateVisual();

         base.OnMouseDown(e);
      }

      protected override void OnMouseUp(MouseButtonEventArgs e)
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

      protected override void OnMouseMove(MouseEventArgs e)
      {
         if(Core.IsDragging)
         {
            Core.mouseCurrent = new System.Drawing.Point((int) e.GetPosition(this).X, (int) e.GetPosition(this).Y);

            if(e.RightButton == MouseButtonState.Pressed)
            {
               Core.Drag(Core.mouseCurrent);
            }
            else if(e.LeftButton == MouseButtonState.Pressed)
            {
               if(CurrentMarkerEnabled)
               {
                  ChangeCurrentPositionOnly(new System.Drawing.Point(Core.mouseDown.X - Core.renderOffset.X, Core.mouseDown.Y - Core.renderOffset.Y));
                  InvalidateVisual();
               }
            }
         }

         base.OnMouseMove(e);
      }
      #endregion

      #region IGMapControl Members

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

      public PointLatLng FromLocalToLatLng(System.Drawing.Point local)
      {
         return Core.FromLocalToLatLng(local);
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

      public void ChangeCurrentPositionOnly(System.Drawing.Point localPoint)
      {
         Core.ChangeCurrentPositionOnly(localPoint);
      }

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

      public System.Drawing.Point CurrentPositionGPixel
      {
         get
         {
            return Core.CurrentPositionGPixel;
         }
      }

      public System.Drawing.Point CurrentPositionGTile
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

      public Font TooltipFont
      {
         get
         {
            return Core.TooltipFont;
         }
         set
         {
            Core.TooltipFont = value;
         }
      }

      public System.Drawing.Size TooltipTextPadding
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

      public GMapNET.RenderMode RenderMode
      {
         get
         {
            return GMapNET.RenderMode.WPF;
         }
      }

      #endregion

      #region IGMapControl event Members

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

   #region purity control
   internal class WindowsPresentationImage : PureImage
   {
      public ImageSource Img;

      public override IntPtr GetHbitmap()
      {
         return IntPtr.Zero;
      }

      public override object Clone()
      {
         if(Img != null)
         {
            WindowsPresentationImage ret = new WindowsPresentationImage();
            ret.Img = Img.CloneCurrentValue();
            return ret;
         }
         return null;
      }

      public override void Dispose()
      {
         if(Img != null)
         {
            Img = null;
         }
      }
   }

   internal class WindowsPresentationImageProxy : PureImageProxy
   {
      public override PureImage FromStream(Stream stream)
      {
         WindowsPresentationImage ret = null;
         if(stream != null)
         {
            try
            {
               ret = new WindowsPresentationImage();
               PngBitmapDecoder bitmapDecoder = new PngBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
               ret.Img = bitmapDecoder.Frames[0];
            }
            catch
            {
               ret = null;
            }
         }
         return ret;
      }

      public override void Save(Stream stream, PureImage image)
      {
         WindowsPresentationImage ret = (WindowsPresentationImage) image;
         if(ret.Img != null)
         {
            PngBitmapEncoder e = new PngBitmapEncoder();
            e.Frames.Add(BitmapFrame.Create(ret.Img as BitmapSource));
            e.Save(stream);
         }
      }
   }
   #endregion
}
