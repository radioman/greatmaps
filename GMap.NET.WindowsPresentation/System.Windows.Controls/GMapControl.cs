
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
   using System.Windows.Media.Animation;
   using System.Windows.Media.Effects;
   using System.Windows.Media.Imaging;
   using System.Windows.Shapes;
   using System.Windows.Threading;
   using GMap.NET;
   using GMap.NET.Internals;
   using GMap.NET.WindowsPresentation;
   using System.Windows.Controls;

   /// <summary>
   /// GMap.NET control for Windows Presentation
   /// </summary>
   public partial class GMapControl : ItemsControl, IGControl
   {
      readonly Core Core = new Core();
      GMap.NET.Rectangle region;
      bool RaiseEmptyTileError = false;
      delegate void MethodInvoker();
      PointLatLng selectionStart;
      PointLatLng selectionEnd;
      Typeface tileTypeface = new Typeface("Arial");
      double zoomReal;
      bool showTileGridLines = false;

      FormattedText googleCopyright;
      FormattedText yahooMapCopyright;
      FormattedText virtualEarthCopyright;
      FormattedText openStreetMapCopyright;
      FormattedText arcGisMapCopyright;

      /// <summary>
      /// pen for empty tile borders
      /// </summary>
      public Pen EmptyTileBorders = new Pen(Brushes.White, 1.0);

      /// <summary>
      /// pen for Selection
      /// </summary>
      public Pen SelectionPen = new Pen(Brushes.Blue, 3.0);

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

      private int maxZoom = 2;

      /// <summary>
      /// max zoom
      /// </summary>         
      [Category("GMap.NET")]
      [Description("maximum zoom level of map")]
      public int MaxZoom
      {
         get
         {
            return maxZoom;
         }
         set
         {
            maxZoom = value;
         }
      }

      private int minZoom = 2;

      /// <summary>
      /// min zoom
      /// </summary>      
      [Category("GMap.NET")]
      [Description("minimum zoom level of map")]
      public int MinZoom
      {
         get
         {
            return minZoom;
         }
         set
         {
            minZoom = value;
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
      /// map dragg button
      /// </summary>
      public MouseButton DragButton = MouseButton.Right;

      /// <summary>
      /// shows tile gridlines
      /// </summary>
      [Category("GMap.NET")]
      public bool ShowTileGridLines
      {
         get
         {
            return showTileGridLines;
         }
         set
         {
            showTileGridLines = value;
            InvalidateVisual();
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
            InvalidateVisual();
         }
      }

      /// <summary>
      /// map boundaries
      /// </summary>
      public RectLatLng? BoundsOfMap = null;

      /// <summary>
      /// list of markers
      /// </summary>
      public readonly ObservableCollection<GMapMarker> Markers = new ObservableCollection<GMapMarker>();

      /// <summary>
      /// current map transformation
      /// </summary>
      internal Transform MapRenderTransform;

      /// <summary>
      /// current markers overlay offset
      /// </summary>
      internal TranslateTransform MapTranslateTransform = new TranslateTransform();

      /// <summary>
      /// map zoom
      /// </summary>
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

              double remainder = value % 1;
              if ((remainder != 0) && (ActualWidth > 0))
               {
                  double scaleValue = remainder + 1;
                  {
                     MapRenderTransform = new ScaleTransform(scaleValue, scaleValue, ActualWidth / 2, ActualHeight / 2);
                  }

                  if(IsLoaded)
                  {
                     //DisplayZoomInFadeImage();
                  }

                  ZoomStep = Convert.ToInt32(value - remainder);

                  Core_OnMapZoomChanged();

                  InvalidateVisual();
               }
               else
               {
                  MapRenderTransform = null;
                  ZoomStep = Convert.ToInt32(value);
                  InvalidateVisual();
               }
            }
         }
      }

      protected bool DesignModeInConstruct
      {
         get
         {
            //Are we in Visual Studio Designer?
            return System.ComponentModel.DesignerProperties.GetIsInDesignMode(this);
         }
      }

      Canvas mapCanvas = null;

      /// <summary>
      /// markers overlay
      /// </summary>
      internal Canvas MapCanvas
      {
         get
         {
            if(mapCanvas == null)
            {
               if(this.VisualChildrenCount > 0)
               {
                  Border border = VisualTreeHelper.GetChild(this, 0) as Border;
                  ItemsPresenter items = border.Child as ItemsPresenter;
                  DependencyObject target = VisualTreeHelper.GetChild(items, 0);
                  mapCanvas = target as Canvas;
               }
            }

            return mapCanvas;
         }
      }

      public GMaps Manager
      {
         get
         {
            return GMaps.Instance;
         }
      }

      public GMapControl()
      {
         if(!DesignModeInConstruct)
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
            //            <Setter Property="Canvas.Left" Value="{Binding Path=LocalPositionX}"/>
            //            <Setter Property="Canvas.Top" Value="{Binding Path=LocalPositionY}"/>
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
               factoryPanel.SetValue(Canvas.IsItemsHostProperty, true);

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
            Core.OnMapZoomChanged += new MapZoomChanged(Core_OnMapZoomChanged);
            Loaded += new RoutedEventHandler(GMapControl_Loaded);
            Unloaded += new RoutedEventHandler(GMapControl_Unloaded);
            SizeChanged += new SizeChangedEventHandler(GMapControl_SizeChanged);

            Markers.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Markers_CollectionChanged);
            this.ItemsSource = Markers;

            googleCopyright = new FormattedText(Core.googleCopyright, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface("GenericSansSerif"), 9, Brushes.Navy);
            yahooMapCopyright = new FormattedText(Core.yahooMapCopyright, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface("GenericSansSerif"), 9, Brushes.Navy);
            virtualEarthCopyright = new FormattedText(Core.virtualEarthCopyright, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface("GenericSansSerif"), 9, Brushes.Navy);
            openStreetMapCopyright = new FormattedText(Core.openStreetMapCopyright, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface("GenericSansSerif"), 9, Brushes.Navy);
            arcGisMapCopyright = new FormattedText(Core.arcGisCopyright, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface("GenericSansSerif"), 9, Brushes.Navy);

            MapType = MapType.GoogleMap;
         }
      }

      void Markers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
      {
         if(e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
         {
            foreach(GMapMarker marker in e.NewItems)
            {
               marker.ForceUpdateLocalPosition(this);
            }
         }
      }

      // testing smooth zoom
      Storyboard mapFadeStoryboard;
      DoubleAnimation oMapFadeAnimation;
      public Image FadeImage = new Image();

      private void SetFadeStoryBoard(int duration)
      {
         mapFadeStoryboard = null;
         oMapFadeAnimation = null;

         mapFadeStoryboard = new Storyboard();
         oMapFadeAnimation = new DoubleAnimation();

         oMapFadeAnimation.From = 1;
         oMapFadeAnimation.To = 0;
         oMapFadeAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(duration));

         mapFadeStoryboard.Children.Add(oMapFadeAnimation);
         Storyboard.SetTarget(oMapFadeAnimation, FadeImage);
         Storyboard.SetTargetProperty(oMapFadeAnimation, new PropertyPath(UIElement.OpacityProperty));
      }

      public void DisplayZoomInFadeImage()
      {
         ImageSource iSrc = ToImageSource();
         FadeImage.Source = iSrc;

         if(!CurrentPosition.IsEmpty)
         {
            GMap.NET.Point p = FromLatLngToLocal(CurrentPosition);
            ScaleTransform scaleTransform = new ScaleTransform(2, 2, p.X, p.Y);
            FadeImage.RenderTransform = scaleTransform;
         }
         mapFadeStoryboard.Begin(this);
      }

      public void DisplayZoomOutFadeImage()
      {
         ImageSource iSrc = ToImageSource();
         FadeImage.Source = iSrc;
         if(!CurrentPosition.IsEmpty)
         {
            GMap.NET.Point p = FromLatLngToLocal(CurrentPosition);
            ScaleTransform scaleTransform = new ScaleTransform(0.5, 0.5, p.X, p.Y);
            FadeImage.RenderTransform = scaleTransform;
         }
         mapFadeStoryboard.Begin(this);
      }

      /// <summary>
      /// inits core system
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void GMapControl_Loaded(object sender, RoutedEventArgs e)
      {
         Core.StartSystem();
         Core_OnMapZoomChanged();
      }

      void GMapControl_Unloaded(object sender, RoutedEventArgs e)
      {
         Core.OnMapClose();
      }

      /// <summary>
      /// recalculates size
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void GMapControl_SizeChanged(object sender, SizeChangedEventArgs e)
      {
         System.Windows.Size constraint = e.NewSize;

         // 50px outside control
         region = new GMap.NET.Rectangle(-50, -50, (int) constraint.Width + 100, (int) constraint.Height + 100);

         Core.OnMapSizeChanged((int) constraint.Width, (int) constraint.Height);

         // keep center on same position
         if(IsLoaded)
         {
            Core.GoToCurrentPosition();

            UpdateMarkersOffset();
         }
      }

      void Core_OnMapZoomChanged()
      {
         UpdateMarkersOffset();

         foreach(var i in Markers)
         {
            i.ForceUpdateLocalPosition(this);
         }

         var routes = Markers.Where(p => p != null && p.Route.Count > 1);
         if(routes != null)
         {
            foreach(var i in routes)
            {
               i.RegenerateRouteShape(this);
            }
         }
      }

      /// <summary>
      /// on core needs invalidation
      /// </summary>
      void Core_OnNeedInvalidation()
      {
         try
         {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Input, new MethodInvoker(InvalidateVisual));
         }
         catch
         {
         }
      }

      /// <summary>
      /// updates markers overlay offset
      /// </summary>
      void UpdateMarkersOffset()
      {
         if(MapCanvas != null)
         {
            if(MapRenderTransform != null)
            {
               var tp = MapRenderTransform.Transform(new System.Windows.Point(Core.renderOffset.X, Core.renderOffset.Y));
               MapTranslateTransform.X = tp.X;
               MapTranslateTransform.Y = tp.Y;
            }
            else
            {
               MapTranslateTransform.X = Core.renderOffset.X;
               MapTranslateTransform.Y = Core.renderOffset.Y;
            }

            MapCanvas.RenderTransform = MapTranslateTransform;
         }
      }

      /// <summary>
      /// render map in WPF
      /// </summary>
      /// <param name="g"></param>
      void DrawMapWPF(DrawingContext g)
      {
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
                  Core.tileRect.X = Core.tilePoint.X * Core.tileRect.Width;
                  Core.tileRect.Y = Core.tilePoint.Y * Core.tileRect.Height;
                  Core.tileRect.Offset(Core.renderOffset);

                  if(region.IntersectsWith(Core.tileRect))
                  {
                     bool found = false;

                     lock(t.Overlays)
                     {
                        foreach(WindowsPresentationImage img in t.Overlays)
                        {
                           if(img != null && img.Img != null)
                           {
                              if(!found)
                                 found = true;

                              g.DrawImage(img.Img, new Rect(Core.tileRect.X, Core.tileRect.Y, Core.tileRect.Width, Core.tileRect.Height));
                           }
                        }
                     }

                     if(ShowTileGridLines)
                     {
                        g.DrawRectangle(null, EmptyTileBorders, new Rect(Core.tileRect.X, Core.tileRect.Y, Core.tileRect.Width, Core.tileRect.Height));

                        if(Core.tilePoint == Core.centerTileXYLocation)
                        {
                           FormattedText TileText = new FormattedText("CENTER:" + Core.tilePoint.ToString(), System.Globalization.CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, tileTypeface, 16, Brushes.Red);
                           g.DrawText(TileText, new System.Windows.Point(Core.tileRect.X + Core.tileRect.Width / 2 - EmptyTileText.Width / 2, Core.tileRect.Y + Core.tileRect.Height / 2 - EmptyTileText.Height / 2));
                        }
                        else
                        {
                           FormattedText TileText = new FormattedText("TILE: " + Core.tilePoint.ToString(), System.Globalization.CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, tileTypeface, 16, Brushes.Red);
                           g.DrawText(TileText, new System.Windows.Point(Core.tileRect.X + Core.tileRect.Width / 2 - EmptyTileText.Width / 2, Core.tileRect.Y + Core.tileRect.Height / 2 - EmptyTileText.Height / 2));
                        }
                     }

                     // add text if tile is missing
                     if(!found)
                     {
                        g.DrawRectangle(EmptytileBrush, EmptyTileBorders, new Rect(Core.tileRect.X, Core.tileRect.Y, Core.tileRect.Width, Core.tileRect.Height));
                        g.DrawText(EmptyTileText, new System.Windows.Point(Core.tileRect.X + Core.tileRect.Width / 2 - EmptyTileText.Width / 2, Core.tileRect.Y + Core.tileRect.Height / 2 - EmptyTileText.Height / 2));

                        if(ShowTileGridLines)
                        {
                           g.DrawRectangle(null, EmptyTileBorders, new Rect(Core.tileRect.X, Core.tileRect.Y, Core.tileRect.Width, Core.tileRect.Height));
                           {
                              FormattedText TileText = new FormattedText("TILE: " + Core.tilePoint.ToString(), System.Globalization.CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, tileTypeface, 16, Brushes.Red);
                              g.DrawText(TileText, new System.Windows.Point(Core.tileRect.X + Core.tileRect.Width / 2 - EmptyTileText.Width / 2, Core.tileRect.Y - EmptyTileText.Height / 2));
                           }
                        }

                        // raise error
                        if(OnEmptyTileError != null)
                        {
                           if(!RaiseEmptyTileError)
                           {
                              RaiseEmptyTileError = true;

                              this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate()
                              {
                                 OnEmptyTileError(t.Zoom, t.Pos);
                              }));
                           }
                        }
                     }
                  }
               }
            }
         }
      }

      /// <summary>
      /// gets image of the current view
      /// </summary>
      /// <returns></returns>
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

         if(bmp.CanFreeze)
         {
            bmp.Freeze();
         }

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

      /// <summary>
      /// sets zoom to max to fit rect
      /// </summary>
      /// <param name="rect">area</param>
      /// <returns></returns>
      public bool SetZoomToFitRect(RectLatLng rect)
      {
         int maxZoom = Core.GetMaxZoomToFitRect(rect);
         if(maxZoom > 0)
         {
            PointLatLng center = new PointLatLng(rect.Lat - (rect.HeightLat / 2), rect.Lng + (rect.WidthLng / 2));
            CurrentPosition = center;

            if(maxZoom > MaxZoom)
            {
               maxZoom = MaxZoom;
            }

            if(ZoomStep != maxZoom)
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
      /// <param name="ZIndex">z index or null to check all</param>
      /// <returns></returns>
      public bool ZoomAndCenterMarkers(int? ZIndex)
      {
         RectLatLng? rect = GetRectOfAllMarkers(ZIndex);
         if(rect.HasValue)
         {
            return SetZoomToFitRect(rect.Value);
         }

         return false;
      }

      /// <summary>
      /// gets rectangle with all objects inside
      /// </summary>
      /// <param name="ZIndex">z index or null to check all</param>
      /// <returns></returns>
      public RectLatLng? GetRectOfAllMarkers(int? ZIndex)
      {
         RectLatLng? ret = null;

         double left = double.MaxValue;
         double top = double.MinValue;
         double right = double.MinValue;
         double bottom = double.MaxValue;
         IEnumerable<GMapMarker> Overlays;

         if(ZIndex.HasValue)
         {
            Overlays = Markers.Where(p => p != null && p.ZIndex == ZIndex);
         }
         else
         {
            Overlays = Markers;
         }

         if(Overlays != null)
         {
            foreach(var m in Overlays)
            {
               if(m.Shape != null && m.Shape.IsVisible)
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

         if(left != double.MaxValue && right != double.MinValue && top != double.MinValue && bottom != double.MaxValue)
         {
            ret = RectLatLng.FromLTRB(left, top, right, bottom);
         }

         return ret;
      }

      /// <summary>
      /// offset position in pixels
      /// </summary>
      /// <param name="x"></param>
      /// <param name="y"></param>
      public void Offset(int x, int y)
      {
         if(IsLoaded)
         {
            Core.DragOffset(new GMap.NET.Point(x, y));
            UpdateMarkersOffset();
         }
      }

      #region UserControl Events
      protected override void OnRender(DrawingContext drawingContext)
      {
         if(MapRenderTransform != null)
         {
            drawingContext.PushTransform(MapRenderTransform);
            {
               DrawMapWPF(drawingContext);
            }
            drawingContext.Pop();
         }
         else
         {
            DrawMapWPF(drawingContext);
         }

         // selection
         if(!SelectedArea.IsEmpty)
         {
            GMap.NET.Point p1 = FromLatLngToLocal(SelectedArea.LocationTopLeft);
            GMap.NET.Point p2 = FromLatLngToLocal(SelectedArea.LocationRightBottom);

            if(MapTranslateTransform != null)
            {
               p1.Offset((int) MapTranslateTransform.X, (int) MapTranslateTransform.Y);
               p2.Offset((int) MapTranslateTransform.X, (int) MapTranslateTransform.Y);
            }

            int x1 = p1.X;
            int y1 = p1.Y;
            int x2 = p2.X;
            int y2 = p2.Y;

            drawingContext.DrawRectangle(null, SelectionPen, new Rect(x1, y1, x2 - x1, y2 - y1));
         }

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
            case MapType.OpenStreetMapSurfer:
            case MapType.OpenStreetMapSurferTerrain:
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

            case MapType.BingHybrid:
            case MapType.BingMap:
            case MapType.BingSatellite:
            {
               drawingContext.DrawText(virtualEarthCopyright, new System.Windows.Point(5, ActualHeight - virtualEarthCopyright.Height - 5));
            }
            break;

            case MapType.ArcGIS_Map:
            case MapType.ArcGIS_Satellite:
            case MapType.ArcGIS_ShadedRelief:
            case MapType.ArcGIS_Terrain:
            case MapType.ArcGIS_MapsLT_OrtoFoto:
            case MapType.ArcGIS_MapsLT_Map:
            case MapType.ArcGIS_MapsLT_Map_Hybrid:
            case MapType.ArcGIS_MapsLT_Map_Labels:
            {
               drawingContext.DrawText(arcGisMapCopyright, new System.Windows.Point(5, ActualHeight - virtualEarthCopyright.Height - 5));
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
            System.Windows.Point p = e.GetPosition(this);

            if(MapRenderTransform != null)
            {
               //p = MapRenderTransform.Inverse.Transform(p);
            }

            if(Core.mouseLastZoom.X != (int) p.X && Core.mouseLastZoom.Y != (int) p.Y)
            {
               if(MouseWheelZoomType == MouseWheelZoomType.MousePositionAndCenter)
               {
                  Core.currentPosition = FromLocalToLatLng((int) p.X, (int) p.Y);
               }
               else if(MouseWheelZoomType == MouseWheelZoomType.ViewCenter)
               {
                  Core.currentPosition = FromLocalToLatLng((int) ActualWidth / 2, (int) ActualHeight / 2);
               }
               else if(MouseWheelZoomType == MouseWheelZoomType.MousePositionWithoutCenter)
               {
                  Core.currentPosition = FromLocalToLatLng((int) p.X, (int) p.Y);
               }

               Core.mouseLastZoom.X = (int) p.X;
               Core.mouseLastZoom.Y = (int) p.Y;
            }

            // set mouse position to map center
            if(MouseWheelZoomType != MouseWheelZoomType.MousePositionWithoutCenter)
            {
               System.Windows.Point ps = PointToScreen(new System.Windows.Point(ActualWidth / 2, ActualHeight / 2));
               Stuff.SetCursorPos((int) ps.X, (int) ps.Y);
            }

            Core.MouseWheelZooming = true;

            if(e.Delta > 0)
            {
               Zoom = ((int) Zoom) + 1;
            }
            else
            {
               if(e.Delta < 0)
               {
                  Zoom = ((int) (Zoom + 0.99)) - 1;
               }
            }

            Core.MouseWheelZooming = false;
         }

         base.OnMouseWheel(e);
      }

      bool isSelected = false;
      protected override void OnMouseDown(MouseButtonEventArgs e)
      {
         if(CanDragMap && e.ChangedButton == DragButton && e.ButtonState == MouseButtonState.Pressed)
         {
            Mouse.Capture(this);

            System.Windows.Point p = e.GetPosition(this);

            if(MapRenderTransform != null)
            {
               p = MapRenderTransform.Inverse.Transform(p);
            }

            Core.mouseDown.X = (int) p.X;
            Core.mouseDown.Y = (int) p.Y;
            {
               Cursor = Cursors.SizeAll;
               Core.BeginDrag(Core.mouseDown);
            }
            InvalidateVisual();
         }
         else
            if(!isSelected)
            {
               System.Windows.Point p = e.GetPosition(this);
               isSelected = true;
               SelectedArea = RectLatLng.Empty;
               selectionEnd = PointLatLng.Empty;
               selectionStart = FromLocalToLatLng((int) p.X, (int) p.Y);
            }

         base.OnMouseDown(e);
      }

      protected override void OnMouseUp(MouseButtonEventArgs e)
      {
         if(isSelected)
         {
            isSelected = false;
         }

         if(Core.IsDragging)
         {
            Mouse.Capture(null);

            Core.EndDrag();
            Cursor = Cursors.Arrow;

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
            if(!selectionEnd.IsEmpty && !selectionStart.IsEmpty)
            {
               if(!SelectedArea.IsEmpty && Keyboard.Modifiers == ModifierKeys.Shift)
               {
                  SetZoomToFitRect(SelectedArea);
               }
            }
            else
            {
               InvalidateVisual();
            }
         }
         RaiseEmptyTileError = false;

         base.OnMouseUp(e);
      }

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
               System.Windows.Point p = e.GetPosition(this);

               if(MapRenderTransform != null)
               {
                  p = MapRenderTransform.Inverse.Transform(p);
               }

               Core.mouseCurrent.X = (int) p.X;
               Core.mouseCurrent.Y = (int) p.Y;
               {
                  Core.Drag(Core.mouseCurrent);
               }

               UpdateMarkersOffset();
            }
         }
         else
         {
            if(isSelected && !selectionStart.IsEmpty && (Keyboard.Modifiers == ModifierKeys.Shift || Keyboard.Modifiers == ModifierKeys.Alt))
            {
               System.Windows.Point p = e.GetPosition(this);
               selectionEnd = FromLocalToLatLng((int) p.X, (int) p.Y);
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
         if(MapRenderTransform != null)
         {
            var tp = MapRenderTransform.Inverse.Transform(new System.Windows.Point(x, y));
            x = (int) tp.X;
            y = (int) tp.Y;
         }

         if(MapTranslateTransform != null)
         {
            //x -= (int) MapTranslateTransform.X;
            //y -= (int) MapTranslateTransform.Y;
         }

         return Core.FromLocalToLatLng(x, y);
      }

      public GMap.NET.Point FromLatLngToLocal(PointLatLng point)
      {
         GMap.NET.Point ret = Core.FromLatLngToLocal(point);

         if(MapRenderTransform != null)
         {
            var tp = MapRenderTransform.Transform(new System.Windows.Point(ret.X, ret.Y));
            ret.X = (int) tp.X;
            ret.Y = (int) tp.Y;
         }

         if(MapTranslateTransform != null)
         {
            ret.Offset(-(int) MapTranslateTransform.X, -(int) MapTranslateTransform.Y);
         }

         return ret;
      }

      public bool ShowExportDialog()
      {
         if(Cache.Instance.ImageCache is GMap.NET.CacheProviders.SQLitePureImageCache)
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
         }
         else
         {
            MessageBox.Show("Failed! Only SQLite support ;/", "GMap.NET", MessageBoxButton.OK, MessageBoxImage.Warning);
         }
         return false;
      }

      public bool ShowImportDialog()
      {
         if(Cache.Instance.ImageCache is GMap.NET.CacheProviders.SQLitePureImageCache)
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
         }
         else
         {
            MessageBox.Show("Failed! Only SQLite support ;/", "GMap.NET", MessageBoxButton.OK, MessageBoxImage.Warning);
         }

         return false;
      }

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
            UpdateMarkersOffset();
         }
      }

      [Browsable(false)]
      public GMap.NET.Point CurrentPositionGPixel
      {
         get
         {
            return Core.CurrentPositionGPixel;
         }
      }

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

      [Browsable(false)]
      public bool IsDragging
      {
         get
         {
            return Core.IsDragging;
         }
      }

      [Browsable(false)]
      public RectLatLng CurrentViewArea
      {
         get
         {
            return Core.CurrentViewArea;
         }
      }

      [Category("GMap.NET")]
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

      [Browsable(false)]
      public PureProjection Projection
      {
         get
         {
            return Core.Projection;
         }
      }

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

      #endregion
   }
}
