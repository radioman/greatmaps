using System.ComponentModel;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Documents;
using System.Globalization;
using GMapNET;
using GMapNET.Internals;

namespace System.Windows.Controls
{
   delegate void MethodInvoker();

   public partial class GMap : UserControl, IGControl
   {
      readonly Core Core = new Core();
      GMapNET.Rectangle region;
      Canvas Canvas = new Canvas();
      public GMapMarker CurrentMarker;
      public int MaxZoom = 19;
      public int MinZoom = 1;

      public GMap()
      {
         Content = Canvas;
         ClipToBounds = true;
         SnapsToDevicePixels = true;

         Purity.Instance.ImageProxy = new WindowsPresentationImageProxy();

         Core.RenderMode = GMapNET.RenderMode.WPF;
         Core.OnNeedInvalidation += new NeedInvalidation(Core_OnNeedInvalidation);
         Core.OnCurrentPositionChanged += new CurrentPositionChanged(Core_OnCurrentPositionChanged);
         Core.OnMapDrag += new MapDrag(UpdateMarkersLocalPositions);
         SizeChanged += new SizeChangedEventHandler(GMap_SizeChanged);
         Loaded += new RoutedEventHandler(GMap_Loaded);

         //GMapMarkerCircle c = new GMapMarkerCircle(this);
         //GMapMarkerRect c = new GMapMarkerRect(this);
         //GMapMarkerTriangle c = new GMapMarkerTriangle(this);
         GMapMarkerCross c = new GMapMarkerCross(this);
         //c.Label.Content = "Maršrutas: 05\nMašina: 1245\nVairuotojas: Jonas P.\nLaikas: 2009.02.02 15:30:42";
         //c.Text = "R1";
         CurrentMarker = c;
         AddMarker(CurrentMarker);
      }

      /// <summary>
      /// update markers location on map drag
      /// </summary>
      public void UpdateMarkersLocalPositions()
      {
         foreach(GMapMarker el in Core.objects)
         {
           el.UpdateLocalPosition(this);
         }
         InvalidateVisual();
      }

      /// <summary>
      /// update current marker
      /// </summary>
      /// <param name="point"></param>
      void Core_OnCurrentPositionChanged(PointLatLng point)
      {
         CurrentMarker.Label.Content = CurrentPosition.ToString();
         CurrentMarker.Position = Core.CurrentPosition;
         CurrentMarker.UpdateLocalPosition(this);
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
         Core.sizeOfMapArea.Width = (int) e.NewSize.Width;
         Core.sizeOfMapArea.Height = (int) e.NewSize.Height;
         Core.sizeOfMapArea.Height /= GMaps.Instance.TileSize.Height;
         Core.sizeOfMapArea.Width /= GMaps.Instance.TileSize.Width;
         Core.sizeOfMapArea.Height += 1;
         Core.sizeOfMapArea.Width += 1;

         Core.sizeOfMapArea.Width = Core.sizeOfMapArea.Width/2 + 2;
         Core.sizeOfMapArea.Height = Core.sizeOfMapArea.Height/2 + 2;

         // 50px outside control
         region = new GMapNET.Rectangle(-50, -50, (int) e.NewSize.Width+100, (int) e.NewSize.Height+100);

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
               if(t != null) // debug center tile add:  && Core.tilePoint != Core.centerTileXYLocation
               {
                  Core.tileRect.X = Core.tilePoint.X*Core.tileRect.Width;
                  Core.tileRect.Y = Core.tilePoint.Y*Core.tileRect.Height;
                  Core.tileRect.Offset(Core.renderOffset);

                  if(region.IntersectsWith(Core.tileRect))
                  {
                     foreach(WindowsPresentationImage img in t.Overlays)
                     {
                        if(img != null && img.Img != null)
                        {
                           g.DrawImage(img.Img, new Rect(Core.tileRect.X, Core.tileRect.Y, Core.tileRect.Width, Core.tileRect.Height));
                        }
                     }
                  }
               }
            }
         }
      }

      // gets image of the current view
      public ImageSource ToImageSource()
      {
         FrameworkElement obj = this;

         // Save current canvas transform
         Transform transform = obj.LayoutTransform;
         obj.LayoutTransform = null;

         // fix margin offset as well
         Thickness margin = obj.Margin;
         obj.Margin = new Thickness(0, 0,
         margin.Right - margin.Left, margin.Bottom - margin.Top);

         // Get the size of canvas
         Size size = new Size(obj.ActualWidth, obj.ActualHeight);

         // force control to Update
         obj.Measure(size);
         obj.Arrange(new Rect(size));

         RenderTargetBitmap bmp = new RenderTargetBitmap(
         (int) size.Width, (int) size.Height, 96, 96, PixelFormats.Pbgra32);

         bmp.Render(obj);

         // return values as they were before
         obj.LayoutTransform = transform;
         obj.Margin = margin;

         return bmp;
      }

      #region UserControl Events
      protected override void OnRender(DrawingContext drawingContext)
      {
         base.OnRender(drawingContext);

         if(Core.RenderMode == GMapNET.RenderMode.WPF)
         {
            DrawMapWPF(drawingContext);
         }
      }

      protected override void OnMouseWheel(MouseWheelEventArgs e)
      {
         if(IsMouseDirectlyOver)
         {
            if(e.Delta > 0)
            {
               int zm = Zoom + 1;
               if(zm <= MaxZoom)
               {
                  Zoom = zm;
               }
            }

            if(e.Delta < 0)
            {
               int zm = Zoom - 1;
               if(zm >= MinZoom)
               {
                  Zoom = zm;
               }
            }
         }

         base.OnMouseWheel(e);
      }

      protected override void OnMouseDown(MouseButtonEventArgs e)
      {
         Point p = e.GetPosition(this);
         Core.mouseDown.X = (int) p.X;
         Core.mouseDown.Y = (int) p.Y;

         if(e.LeftButton == MouseButtonState.Pressed)
         {
            if(CurrentMarkerEnabled && !IsMouseOverMarker)
            {
               SetCurrentPositionOnly(Core.mouseDown.X - Core.renderOffset.X, Core.mouseDown.Y - Core.renderOffset.Y);

               if(Core.MouseVisible)
               {
                  //Cursor = Cursors.None;
                  Core.MouseVisible = false;
               }

               Core.BeginDrag(Core.mouseDown);
            }
         }
         else if(e.RightButton == MouseButtonState.Pressed)
         {
            if(CanDragMap)
            {
               Cursor = Cursors.SizeAll;
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

            Cursor = Cursors.Arrow;

            if(!Core.MouseVisible)
            {
               Core.MouseVisible = true;
            }
         }

         base.OnMouseUp(e);
      }

      protected override void OnMouseMove(MouseEventArgs e)
      {
         if(Core.IsDragging)
         {
            Point p = e.GetPosition(this);
            Core.mouseCurrent.X = (int) p.X;
            Core.mouseCurrent.Y = (int) p.Y;

            if(e.RightButton == MouseButtonState.Pressed)
            {
               Core.Drag(Core.mouseCurrent);
            }
            else if(e.LeftButton == MouseButtonState.Pressed)
            {
               if(CurrentMarkerEnabled)
               {
                  SetCurrentPositionOnly(Core.mouseCurrent.X - Core.renderOffset.X, Core.mouseCurrent.Y - Core.renderOffset.Y);
                  InvalidateVisual();
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

      public void AddMarker(MapObject item)
      {
         foreach(KeyValuePair<UIElement, Point> el in (item as GMapMarker).Objects)
         {
            Canvas.Children.Add(el.Key);
         }
         Core.AddMarker(item);
      }

      public void RemoveMarker(MapObject item)
      {
         foreach(KeyValuePair<UIElement, Point> el in (item as GMapMarker).Objects)
         {
            Canvas.Children.Remove(el.Key);
         }
         Core.RemoveMarker(item);
      }

      public void ClearAllMarkers()
      {
         Canvas.Children.Clear();
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
         Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
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

            if(dlg.ShowDialog() == true)
            {
               bool ok = GMaps.Instance.ExportToGMDB(dlg.FileName);
               if(ok)
               {
                  MessageBox.Show("Complete!", "GMap.NET", MessageBoxButton.OK, MessageBoxImage.Information);
               }
               else
               {
                  MessageBox.Show("  Failed!", "GMap.NET", MessageBoxButton.OK, MessageBoxImage.Warning);
               }

               return ok;
            }
         }
         return false;
      }

      public bool ShowImportDialog()
      {
         Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
         {
            dlg.CheckPathExists = true;
            dlg.CheckFileExists = false;
            dlg.AddExtension = true;
            dlg.DefaultExt = "gmdb";
            dlg.ValidateNames = true;
            dlg.Title = "GMap.NET: Import to db, only new data will be added";
            dlg.FileName = "DataImport";
            dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            dlg.Filter = "GMap.NET DB files (*.gmdb)|*.gmdb";
            dlg.FilterIndex = 1;
            dlg.RestoreDirectory = true;

            if(dlg.ShowDialog() == true)
            {
               Cursor = Cursors.Wait;

               bool ok = GMaps.Instance.ImportFromGMDB(dlg.FileName);
               if(ok)
               {
                  MessageBox.Show("Complete!", "GMap.NET", MessageBoxButton.OK, MessageBoxImage.Information);
                  ReloadMap();
               }
               else
               {
                  MessageBox.Show("  Failed!", "GMap.NET", MessageBoxButton.OK, MessageBoxImage.Warning);
               }

               Cursor = Cursors.Arrow;

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
            CurrentMarker.Shape.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
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

      public GMapNET.RenderMode RenderMode
      {
         get
         {
            return GMapNET.RenderMode.WPF;
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
            {
               // try png decoder
               try
               {
                  PngBitmapDecoder bitmapDecoder = new PngBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                  ImageSource m = bitmapDecoder.Frames[0];

                  if(m != null)
                  {
                     ret = new WindowsPresentationImage();
                     ret.Img = m;
                  }
               }
               catch
               {
                  ret = null;
               }

               // try jpeg decoder
               if(ret == null)
               {
                  try
                  {
                     stream.Seek(0, SeekOrigin.Begin);

                     JpegBitmapDecoder bitmapDecoder = new JpegBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                     ImageSource m = bitmapDecoder.Frames[0];

                     if(m != null)
                     {
                        ret = new WindowsPresentationImage();
                        ret.Img = m;
                     }
                  }
                  catch
                  {
                     ret = null;
                  }
               }
            }
         }
         return ret;
      }

      public override bool Save(Stream stream, PureImage image)
      {
         WindowsPresentationImage ret = (WindowsPresentationImage) image;
         if(ret.Img != null)
         {
            try
            {
               PngBitmapEncoder e = new PngBitmapEncoder();
               e.Frames.Add(BitmapFrame.Create(ret.Img as BitmapSource));
               e.Save(stream);
            }
            catch
            {
               return false;
            }
         }
         else
         {
            return false;
         }

         return true;
      }
   }
   #endregion
}
