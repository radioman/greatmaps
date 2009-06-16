
namespace System.Windows.Controls
{
   using System.Collections.Generic;
   using System.Collections.ObjectModel;
   using System.ComponentModel;
   using System.Globalization;
   using System.Linq;
   using System.Windows;
   using System.Windows.Data;
   using System.Windows.Input;
   using System.Windows.Media;
   using System.Windows.Media.Effects;
   using System.Windows.Media.Imaging;
   using System.Windows.Shapes;
   using System.Windows.Threading;
   using GMap.NET;
   using GMap.NET.Internals;
   using GMap.NET.WindowsPresentation;
   using System.Diagnostics;

   /// <summary>
   /// GMap.NET control for Windows Presentation
   /// </summary>
   public partial class GMapControl : ItemsControl, IGControl
   {
      readonly Core Core = new Core();
      GMap.NET.Rectangle region;
      bool RaiseEmptyTileError = false;
      delegate void MethodInvoker();

      FormattedText googleCopyright;
      FormattedText yahooMapCopyright;
      FormattedText virtualEarthCopyright;
      FormattedText openStreetMapCopyright;

      /// <summary>
      /// pen for empty tile borders
      /// </summary>
      public Pen EmptyTileBorders = new Pen(Brushes.White, 1.0);

      /// <summary>
      /// /// <summary>
      /// pen for empty tile background
      /// </summary>
      public Brush EmptytileBrush = Brushes.Navy;

      /// <summary>
      /// occurs on empty tile displayed
      /// </summary>
      public event EmptyTileError OnEmptyTileError;

      /// <summary>
      /// text on empty tiles
      /// </summary>
      public FormattedText EmptyTileText = new FormattedText("We are sorry, but we don't\nhave imagery at this zoom\n     level for this region.", System.Globalization.CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface("Arial"), 16, Brushes.White);

      /// <summary>
      /// max zoom
      /// </summary>
      public int MaxZoom;

      /// <summary>
      /// min zoom
      /// </summary>
      public int MinZoom;

      /// <summary>
      /// map zooming type for mouse wheel
      /// </summary>
      public MouseWheelZoomType MouseWheelZoomType = MouseWheelZoomType.MousePosition;

      /// <summary>
      /// NOT WORK YET
      /// where to set current position if map size is changed
      /// </summary>
      public SizeChangedType SizeChangedType = SizeChangedType.ViewCenter;

      /// <summary>
      /// center mouse OnMouseWheel
      /// </summary>
      public bool CenterPositionOnMouseWheel = true;

      /// <summary>
      /// map dragg button
      /// </summary>
      public MouseButton DragButton = MouseButton.Right;

      /// <summary>
      /// list of markers
      /// </summary>
      public readonly ObservableCollection<GMapMarker> Markers = new ObservableCollection<GMapMarker>();

      public GMapControl()
      {
         #region -- templates --

         #region -- xaml --
         //  <ItemsControl Name="figures">
         //    <ItemsControl.ItemTemplate>
         //        <DataTemplate>
         //            <ContentPresenter Content="{Binding Path=Shape}" />
         //        </DataTemplate>
         //    </ItemsControl.ItemTemplate>
         //    <ItemsControl.ItemsPanel>
         //        <ItemsPanelTemplate>
         //            <Canvas />
         //        </ItemsPanelTemplate>
         //    </ItemsControl.ItemsPanel>
         //    <ItemsControl.ItemContainerStyle>
         //        <Style>
         //            <Setter Property="Canvas.Left" Value="{Binding Path=Pos.X}"/>
         //            <Setter Property="Canvas.Top" Value="{Binding Path=Pos.Y}"/>
         //        </Style>
         //    </ItemsControl.ItemContainerStyle>
         //</ItemsControl> 
         #endregion

         DataTemplate dt = new DataTemplate(typeof(GMapMarker));
         {
            FrameworkElementFactory fef = new FrameworkElementFactory(typeof(ContentPresenter));
            fef.SetBinding(ContentPresenter.ContentProperty, new Binding("Shape"));
            dt.VisualTree = fef;
         }
         ItemTemplate = dt;

         FrameworkElementFactory factoryPanel = new FrameworkElementFactory(typeof(Canvas));
         {
            ItemsPanelTemplate template = new ItemsPanelTemplate();
            template.VisualTree = factoryPanel;
            ItemsPanel = template;           
         }

         Style st = new Style();
         {
            st.Setters.Add(new Setter(Canvas.LeftProperty, new Binding("LocalPositionX")));
            st.Setters.Add(new Setter(Canvas.TopProperty, new Binding("LocalPositionY")));
            st.Setters.Add(new Setter(Canvas.ZIndexProperty, new Binding("ZIndex")));
         }
         ItemContainerStyle = st;
         #endregion

         ClipToBounds = true;
         SnapsToDevicePixels = true;

         // removes white lines between tiles!
         SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);

         GMaps.Instance.ImageProxy = new WindowsPresentationImageProxy();

         Core.RenderMode = GMap.NET.RenderMode.WPF;
         Core.OnNeedInvalidation += new NeedInvalidation(Core_OnNeedInvalidation);
         Core.OnMapDrag += new MapDrag(Core_OnMapDrag);
         Core.OnMapZoomChanged += new MapZoomChanged(Core_OnMapZoomChanged);
         Loaded += new RoutedEventHandler(GMapControl_Loaded);
         SizeChanged += new SizeChangedEventHandler(GMapControl_SizeChanged);
         
         this.ItemsSource = Markers;

         googleCopyright = new FormattedText(Core.googleCopyright, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface("GenericSansSerif"), 9, Brushes.Navy);
         yahooMapCopyright = new FormattedText(Core.yahooMapCopyright, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface("GenericSansSerif"), 9, Brushes.Navy);
         virtualEarthCopyright = new FormattedText(Core.virtualEarthCopyright, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface("GenericSansSerif"), 9, Brushes.Navy);
         openStreetMapCopyright = new FormattedText(Core.openStreetMapCopyright, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface("GenericSansSerif"), 9, Brushes.Navy);
      }

      /// <summary>
      /// inits core system
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void GMapControl_Loaded(object sender, RoutedEventArgs e)
      {
         Core.StartSystem();
      }

      /// <summary>
      /// recalculates size
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void GMapControl_SizeChanged(object sender, SizeChangedEventArgs e)
      {
         System.Windows.Size constraint = e.NewSize;

         Core.sizeOfMapArea.Width = 1 + ((int) constraint.Width/GMaps.Instance.TileSize.Width)/2;
         Core.sizeOfMapArea.Height = 1 + ((int) constraint.Height/GMaps.Instance.TileSize.Height)/2;

         // 50px outside control
         region = new GMap.NET.Rectangle(-50, -50, (int) constraint.Width+100, (int) constraint.Height+100);

         // keep center on same position
         if(IsLoaded)
         {
            if(SizeChangedType == SizeChangedType.CurrentPosition)
            {
               Core.renderOffset.Offset(((int) constraint.Width-Core.Width)/2, ((int) constraint.Height-Core.Height)/2);
               Core.GoToCurrentPosition();
            }
            else if(SizeChangedType == SizeChangedType.ViewCenter)
            {
               // do not work as expected ;/

               //CurrentPosition = FromLocalToLatLng((int) constraint.Width/2, (int) constraint.Height/2);
               //Core.CurrentPositionGPixel = GMaps.Instance.FromLatLngToPixel(Core.currentPosition, Zoom);
               //Core.CurrentPositionGTile = GMaps.Instance.FromPixelToTileXY(CurrentPositionGPixel);

               //GoToCurrentPosition();


               //Core.renderOffset.Offset(((int) e.NewSize.Width-Core.Width)/2, ((int) e.NewSize.Height-Core.Height)/2);
               //Core.CurrentPosition = FromLocalToLatLng((int) Width/2, (int) Height/2);

               //Core.GoToCurrentPosition();
            }
         }

         Core.OnMapSizeChanged((int) constraint.Width, (int) constraint.Height);
      }

      void Core_OnMapZoomChanged()
      {
         var routes = Markers.Where(p => p != null && p.Route.Count > 1);
         if(routes != null)
         {
            foreach(var i in routes)
            {
               i.RegenerateRouteShape();
            }
         }
      }

      void Core_OnMapDrag()
      {
         foreach(GMapMarker obj in Markers)
         {
            if(obj != null)
            {
               obj.UpdateLocalPosition();
            }
         }
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
      /// render map in WPF
      /// </summary>
      /// <param name="g"></param>
      void DrawMapWPF(DrawingContext g)
      {
         double scaleFactor = 1.0;

         for(int i = -Core.sizeOfMapArea.Width; i <= Core.sizeOfMapArea.Width; i++)
         {
            for(int j = -Core.sizeOfMapArea.Height; j <= Core.sizeOfMapArea.Height; j++)
            {
               Core.tilePoint = Core.centerTileXYLocation;
               Core.tilePoint.X += i;
               Core.tilePoint.Y += j;

               Tile t = Core.Matrix[Core.tilePoint];
               if(t != null)
               {
                  Core.tileRect.X = Core.tilePoint.X*Core.tileRect.Width;
                  Core.tileRect.Y = Core.tilePoint.Y*Core.tileRect.Height;
                  Core.tileRect.Offset(Core.renderOffset);

                  if(region.IntersectsWith(Core.tileRect))
                  {
                     bool found = false;

                     foreach(WindowsPresentationImage img in t.Overlays)
                     {
                        if(img != null && img.Img != null)
                        {
                           if(!found)
                              found = true;

                           g.DrawImage(img.Img, new Rect(Core.tileRect.X*scaleFactor, Core.tileRect.Y*scaleFactor, Core.tileRect.Width*scaleFactor, Core.tileRect.Height*scaleFactor));
                        }
                     }

                     // add text if tile is missing
                     if(!found)
                     {
                        g.DrawRectangle(EmptytileBrush, EmptyTileBorders, new Rect(Core.tileRect.X, Core.tileRect.Y, Core.tileRect.Width, Core.tileRect.Height));
                        g.DrawText(EmptyTileText, new System.Windows.Point(Core.tileRect.X + Core.tileRect.Width/2 - EmptyTileText.Width/2, Core.tileRect.Y + Core.tileRect.Height/2 - EmptyTileText.Height/2));

                        // raise error
                        if(OnEmptyTileError != null)
                        {
                           if(!RaiseEmptyTileError)
                           {
                              RaiseEmptyTileError = true;
                              OnEmptyTileError(t.Zoom, t.Pos);
                           }
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
         System.Windows.Size size = new System.Windows.Size(obj.ActualWidth, obj.ActualHeight);

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

      /// <summary>
      /// creates path from list of points
      /// </summary>
      /// <param name="pl"></param>
      /// <returns></returns>
      public Path CreateRoutePath(List<System.Windows.Point> localPath)
      {
         // Create a StreamGeometry to use to specify myPath.
         StreamGeometry geometry = new StreamGeometry();

         using(StreamGeometryContext ctx = geometry.Open())
         {
            ctx.BeginFigure(localPath[0], false, false);

            // Draw a line to the next specified point.
            ctx.PolyLineTo(localPath, true, true);
         }

         // Freeze the geometry (make it unmodifiable)
         // for additional performance benefits.
         geometry.Freeze();

         // Create a path to draw a geometry with.
         Path myPath = new Path();
         {
            // Specify the shape of the Path using the StreamGeometry.
            myPath.Data = geometry;

            BlurEffect ef = new BlurEffect();
            {
               ef.KernelType = KernelType.Gaussian;
               ef.Radius = 3.0;
               ef.RenderingBias = RenderingBias.Quality;
            }

            myPath.Effect = ef;

            myPath.Stroke = Brushes.Navy;
            myPath.StrokeThickness = 5;
            myPath.StrokeLineJoin = PenLineJoin.Round;
            myPath.StrokeStartLineCap = PenLineCap.Triangle;
            myPath.StrokeEndLineCap = PenLineCap.Square;
            myPath.Opacity = 0.6;
         }
         return myPath;
      }

      #region UserControl Events
      protected override void OnRender(DrawingContext drawingContext)
      {
         DrawMapWPF(drawingContext);

         #region -- copyright --

         switch(Core.MapType)
         {
            case MapType.GoogleMap:
            case MapType.GoogleSatellite:
            case MapType.GoogleLabels:
            case MapType.GoogleTerrain:
            case MapType.GoogleHybrid:
            {
               drawingContext.DrawText(googleCopyright, new System.Windows.Point(5, ActualHeight - googleCopyright.Height - 5));
            }
            break;

            case MapType.OpenStreetMap:
            case MapType.OpenStreetOsm:
            {
               drawingContext.DrawText(openStreetMapCopyright, new System.Windows.Point(5, ActualHeight - openStreetMapCopyright.Height - 5));
            }
            break;

            case MapType.YahooMap:
            case MapType.YahooSatellite:
            case MapType.YahooLabels:
            case MapType.YahooHybrid:
            {
               drawingContext.DrawText(yahooMapCopyright, new System.Windows.Point(5, ActualHeight - yahooMapCopyright.Height - 5));
            }
            break;

            case MapType.VirtualEarthHybrid:
            case MapType.VirtualEarthMap:
            case MapType.VirtualEarthSatellite:
            {
               drawingContext.DrawText(virtualEarthCopyright, new System.Windows.Point(5, ActualHeight - virtualEarthCopyright.Height - 5));
            }
            break;
         }

         #endregion

         base.OnRender(drawingContext);
      }

      protected override void OnMouseWheel(MouseWheelEventArgs e)
      {
         base.OnMouseWheel(e);

         if(IsMouseDirectlyOver && !IsDragging)
         {
            if(MouseWheelZoomType == MouseWheelZoomType.MousePosition)
            {
               System.Windows.Point pl = e.GetPosition(this);
               Core.currentPosition = FromLocalToLatLng((int) pl.X, (int) pl.Y);
            }
            else if(MouseWheelZoomType == MouseWheelZoomType.ViewCenter)
            {
               Core.currentPosition = FromLocalToLatLng((int) ActualWidth/2, (int) ActualHeight/2);
            }

            // set mouse position to map center
            if(CenterPositionOnMouseWheel)
            {
               System.Windows.Point p = PointToScreen(new System.Windows.Point(ActualWidth/2, ActualHeight/2));
               Stuff.SetCursorPos((int) p.X, (int) p.Y);
            }

            if(e.Delta > 0)
            {
               Zoom++;
            }
            else if(e.Delta < 0)
            {
               Zoom--;
            } 
         }

         base.OnMouseWheel(e);
      }        

      protected override void OnMouseDown(MouseButtonEventArgs e)
      {
         if(CanDragMap && e.ChangedButton == DragButton && e.ButtonState == MouseButtonState.Pressed)
         {
            System.Windows.Point p = e.GetPosition(this);
            Core.mouseDown.X = (int) p.X;
            Core.mouseDown.Y = (int) p.Y;
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
         }

         RaiseEmptyTileError = false;

         base.OnMouseUp(e);
      }

      protected override void OnMouseMove(MouseEventArgs e)
      {
         if(Core.IsDragging)
         {
            System.Windows.Point p = e.GetPosition(this);
            Core.mouseCurrent.X = (int) p.X;
            Core.mouseCurrent.Y = (int) p.Y;
            {
               Core.Drag(Core.mouseCurrent);
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

      public GeoCoderStatusCode SetCurrentPositionByKeywords(string keys)
      {
         return Core.SetCurrentPositionByKeywords(keys);
      }

      public PointLatLng FromLocalToLatLng(int x, int y)
      {
         return Core.FromLocalToLatLng(x, y);
      }

      public GMap.NET.Point FromLatLngToLocal(PointLatLng point)
      {
         return Core.FromLatLngToLocal(point);
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
            if(value <= MaxZoom && value >= MinZoom)
            {
               Core.Zoom = value;
            }
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

      public GMap.NET.Point CurrentPositionGPixel
      {
         get
         {
            return Core.CurrentPositionGPixel;
         }
      }

      public GMap.NET.Point CurrentPositionGTile
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

      public RectLatLng CurrentViewArea
      {
         get
         {
            return Core.CurrentViewArea;
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

      public GMap.NET.RenderMode RenderMode
      {
         get
         {
            return GMap.NET.RenderMode.WPF;
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
}
