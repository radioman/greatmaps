
namespace GMap.NET.WindowsPresentation
{
   using System;
   using System.Collections.Generic;
   using System.Collections.ObjectModel;
   using System.ComponentModel;
   using System.Globalization;
   using System.Linq;
   using System.Windows;
   using System.Windows.Controls;
   using System.Windows.Data;
   using System.Windows.Input;
   using System.Windows.Media;
   using System.Windows.Media.Effects;
   using System.Windows.Media.Imaging;
   using System.Windows.Shapes;
   using System.Windows.Threading;
   using GMap.NET;
   using GMap.NET.Internals;
   using System.Diagnostics;
   using GMap.NET.MapProviders;
   using System.Windows.Media.Animation;

   /// <summary>
   /// GMap.NET control for Windows Presentation
   /// </summary>
   public partial class GMapControl : ItemsControl, Interface
   {
      #region DependencyProperties and related stuff

      public System.Windows.Point MapPoint
      {
         get
         {
            return (System.Windows.Point)GetValue(MapPointProperty);
         }
         set
         {
            SetValue(MapPointProperty, value);
         }
      }


      // Using a DependencyProperty as the backing store for point.  This enables animation, styling, binding, etc...
      public static readonly DependencyProperty MapPointProperty =
             DependencyProperty.Register("MapPoint", typeof(System.Windows.Point), typeof(GMapControl), new PropertyMetadata(new Point(), OnMapPointPropertyChanged));


      private static void OnMapPointPropertyChanged(DependencyObject source,
      DependencyPropertyChangedEventArgs e)
      {
         Point temp = (Point)e.NewValue;
         (source as GMapControl).Position = new PointLatLng(temp.X, temp.Y);
      }

      public static readonly DependencyProperty MapProviderProperty = DependencyProperty.Register("MapProvider", typeof(GMapProvider), typeof(GMapControl), new UIPropertyMetadata(EmptyProvider.Instance, new PropertyChangedCallback(MapProviderPropertyChanged)));

      /// <summary>
      /// type of map
      /// </summary>
      [Category("GMap.NET")]
      public GMapProvider MapProvider
      {
         get
         {
            return GetValue(MapProviderProperty) as GMapProvider;
         }
         set
         {
            SetValue(MapProviderProperty, value);
         }
      }

      private static void MapProviderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
      {
         GMapControl map = (GMapControl)d;
         if(map != null)
         {
            Debug.WriteLine("MapType: " + e.OldValue + " -> " + e.NewValue);

            RectLatLng viewarea = map.SelectedArea;
            if(viewarea != RectLatLng.Empty)
            {
               map.Position = new PointLatLng(viewarea.Lat - viewarea.HeightLat / 2, viewarea.Lng + viewarea.WidthLng / 2);
            }
            else
            {
               viewarea = map.CurrentViewArea;
            }

            map.Core.Provider = e.NewValue as GMapProvider;

            map.Copyright = null;
            if(!string.IsNullOrEmpty(map.Core.Provider.Copyright))
            {
               map.Copyright = new FormattedText(map.Core.Provider.Copyright, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface("GenericSansSerif"), 9, Brushes.Navy);
            }

            if(map.Core.IsStarted && map.Core.zoomToArea)
            {
               // restore zoomrect as close as possible
               if(viewarea != RectLatLng.Empty && viewarea != map.CurrentViewArea)
               {
                  int bestZoom = map.Core.GetMaxZoomToFitRect(viewarea);
                  if(bestZoom > 0 && map.Zoom != bestZoom)
                  {
                     map.Zoom = bestZoom;
                  }
               }
            }
         }
      }

      public static readonly DependencyProperty ZoomProperty = DependencyProperty.Register("Zoom", typeof(double), typeof(GMapControl), new UIPropertyMetadata(0.0, new PropertyChangedCallback(ZoomPropertyChanged), new CoerceValueCallback(OnCoerceZoom)));

      /// <summary>
      /// map zoom
      /// </summary>
      [Category("GMap.NET")]
      public double Zoom
      {
         get
         {
            return (double)(GetValue(ZoomProperty));
         }
         set
         {
            SetValue(ZoomProperty, value);
         }
      }

      private static object OnCoerceZoom(DependencyObject o, object value)
      {
         GMapControl map = o as GMapControl;
         if(map != null)
         {
            double result = (double)value;
            if(result > map.MaxZoom)
            {
               result = map.MaxZoom;
            }
            if(result < map.MinZoom)
            {
               result = map.MinZoom;
            }

            return result;
         }
         else
         {
            return value;
         }
      }

      private static void ZoomPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
      {
         GMapControl map = (GMapControl)d;
         if(map != null && map.MapProvider.Projection != null)
         {
            double value = (double)e.NewValue;

            Debug.WriteLine("Zoom: " + e.OldValue + " -> " + value);

            double remainder = value % 1;
            if(remainder != 0 && map.ActualWidth > 0)
            {
               double scaleValue = remainder + 1;
               {
                  map.MapRenderTransform = new ScaleTransform(scaleValue, scaleValue, map.ActualWidth / 2, map.ActualHeight / 2);
               }

               map.Core.Zoom = Convert.ToInt32(value - remainder);

               map.Core_OnMapZoomChanged();

               map.InvalidateVisual(true);
            }
            else
            {
               map.MapRenderTransform = null;
               map.Core.Zoom = Convert.ToInt32(value);
               map.InvalidateVisual(true);
            }
         }
      }

      #endregion

      readonly Core Core = new Core();
      GRect region;
      delegate void MethodInvoker();
      PointLatLng selectionStart;
      PointLatLng selectionEnd;
      Typeface tileTypeface = new Typeface("Arial");
      bool showTileGridLines = false;
      //MethodInvoker invalidator;

      FormattedText Copyright;

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
      /// pen for empty tile borders
      /// </summary>
      public Pen EmptyTileBorders = new Pen(Brushes.White, 1.0);

      /// <summary>
      /// pen for Selection
      /// </summary>
      public Pen SelectionPen = new Pen(Brushes.Blue, 2.0);

      /// <summary>
      /// background of selected area
      /// </summary>
      public Brush SelectedAreaFill = new SolidColorBrush(Color.FromArgb(33, Colors.RoyalBlue.R, Colors.RoyalBlue.G, Colors.RoyalBlue.B));

      /// <summary>
      /// /// <summary>
      /// pen for empty tile background
      /// </summary>
      public Brush EmptytileBrush = Brushes.Navy;

      /// <summary>
      /// text on empty tiles
      /// </summary>
      public FormattedText EmptyTileText = new FormattedText("We are sorry, but we don't\nhave imagery at this zoom\n     level for this region.", System.Globalization.CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface("Arial"), 16, Brushes.Blue);

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
      [Category("GMap.NET")]
      public MouseButton DragButton = MouseButton.Right;

      /// <summary>
      /// use circle for selection
      /// </summary>
      public bool SelectionUseCircle = false;

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
      /// retry count to get tile 
      /// </summary>
      [Browsable(false)]
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
      [Browsable(false)]
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
      /// is touch control enabled
      /// </summary>
      bool TouchEnabled = true;

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
      internal readonly TranslateTransform MapTranslateTransform = new TranslateTransform();

      protected bool DesignModeInConstruct
      {
         get
         {
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

                  mapCanvas.RenderTransform = MapTranslateTransform;
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

            Manager.SQLitePing();

            ClipToBounds = true;
            SnapsToDevicePixels = true;

            GMapProvider.TileImageProxy = new WindowsPresentationImageProxy();

            Core.SystemType = "WindowsPresentation";

            Core.RenderMode = GMap.NET.RenderMode.WPF;
            Core.OnMapZoomChanged += new MapZoomChanged(Core_OnMapZoomChanged);
            Loaded += new RoutedEventHandler(GMapControl_Loaded);
            Unloaded += new RoutedEventHandler(GMapControl_Unloaded);
            SizeChanged += new SizeChangedEventHandler(GMapControl_SizeChanged);

            // by default its internal property, feel free to use your own
            if(ItemsSource == null)
            {
               ItemsSource = Markers;
            }

            Core.Zoom = (int)((double)ZoomProperty.DefaultMetadata.DefaultValue);
         }
      }

      void invalidatorEngage(object sender, ProgressChangedEventArgs e)
      {
         base.InvalidateVisual();
      }

      /// <summary>
      /// enque built-in thread safe invalidation
      /// </summary>
      public new void InvalidateVisual()
      {
         Core.Refresh.Set();
      }

      /// <summary>
      /// Invalidates the rendering of the element, and forces a complete new layout
      /// pass. System.Windows.UIElement.OnRender(System.Windows.Media.DrawingContext)
      /// is called after the layout cycle is completed. If not forced enques built-in thread safe invalidation
      /// </summary>
      /// <param name="forced"></param>
      public void InvalidateVisual(bool forced)
      {
         if(forced)
         {
            lock(Core.invalidationLock)
            {
               Core.lastInvalidation = DateTime.Now;
            }
            base.InvalidateVisual();
         }
         else
         {
            InvalidateVisual();
         }
      }

      protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
      {
         if(e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
         {
            foreach(GMapMarker marker in e.NewItems)
            {
               marker.ForceUpdateLocalPosition(this);
            }
         }

         base.OnItemsChanged(e);
      }

      /// <summary>
      /// inits core system
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void GMapControl_Loaded(object sender, RoutedEventArgs e)
      {
         Core.OnMapOpen().ProgressChanged += new ProgressChangedEventHandler(invalidatorEngage);
         Core_OnMapZoomChanged();

         if(Application.Current != null)
         {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.ApplicationIdle,
               new Action(delegate()
               {
                  if(Application.Current != null)
                  {
                     Application.Current.SessionEnding += new SessionEndingCancelEventHandler(Current_SessionEnding);
                  }
               }
               ));
         }
      }

      void Current_SessionEnding(object sender, SessionEndingCancelEventArgs e)
      {
         GMaps.Instance.CancelTileCaching();
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
         region = new GRect(-50, -50, (int)constraint.Width + 100, (int)constraint.Height + 100);

         Core.OnMapSizeChanged((int)constraint.Width, (int)constraint.Height);

         // keep center on same position
         if(IsLoaded)
         {
            Core.GoToCurrentPosition();

            if(IsRotated)
            {
               UpdateRotationMatrix();
               Core_OnMapZoomChanged();
            }
            else
            {
               UpdateMarkersOffset();
            }
         }
      }

      void Core_OnMapZoomChanged()
      {
         UpdateMarkersOffset();

         foreach(GMapMarker i in ItemsSource)
         {
            if(i != null)
            {
               i.ForceUpdateLocalPosition(this);

               if(i.Route.Count > 0)
               {
                  i.RegenerateRouteShape(this);
               }

               if(i.Polygon.Count > 0)
               {
                  i.RegeneratePolygonShape(this);
               }
            }
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
         }
      }

      public Brush EmptyMapBackground = Brushes.WhiteSmoke;

      /// <summary>
      /// render map in WPF
      /// </summary>
      /// <param name="g"></param>
      void DrawMapWPF(DrawingContext g)
      {
         if(MapProvider == EmptyProvider.Instance || MapProvider == null)
         {
            return;
         }

         Core.Matrix.EnterReadLock();
         Core.tileDrawingListLock.AcquireReaderLock();
         try
         {
            foreach(var tilePoint in Core.tileDrawingList)
            {
               Core.tileRect.Location = tilePoint.PosXY;
               Core.tileRect.Offset(Core.renderOffset);
               Core.tileRect.OffsetNegative(Core.compensationOffset);

               if(region.IntersectsWith(Core.tileRect) || IsRotated)
               {
                  bool found = false;

                  Tile t = Core.Matrix.GetTileWithNoLock(Core.Zoom, tilePoint.PosXY);
                  if(t != Tile.Empty)
                  {
                     lock(t.Overlays)
                     {
                        foreach(WindowsPresentationImage img in t.Overlays)
                        {
                           if(img != null && img.Img != null)
                           {
                              if(!found)
                                 found = true;

                              g.DrawImage(img.Img, new Rect(Core.tileRect.X + 0.6, Core.tileRect.Y + 0.6, Core.tileRect.Width + 0.6, Core.tileRect.Height + 0.6));
                           }
                        }
                     }
                  }
                  //else // testing smooth zooming
                  //{
                  //   int ZoomOffset = 0;
                  //   Tile ParentTile = null;
                  //   int Ix = 0;

                  //   while(ParentTile == null && (Core.Zoom - ZoomOffset) >= 1 && ZoomOffset <= LevelsKeepInMemmory)
                  //   {
                  //      Ix = (int) Math.Pow(2, ++ZoomOffset);
                  //      ParentTile = Core.Matrix.GetTileWithNoLock(Core.Zoom - ZoomOffset, new GMap.NET.Point((int) (tilePoint.X / Ix), (int) (tilePoint.Y / Ix)));
                  //   }

                  //   if(ParentTile != null)
                  //   {
                  //      int Xoff = Math.Abs(tilePoint.X - (ParentTile.Pos.X * Ix));
                  //      int Yoff = Math.Abs(tilePoint.Y - (ParentTile.Pos.Y * Ix));

                  //      // render tile 
                  //      lock(ParentTile.Overlays)
                  //      {
                  //         foreach(WindowsPresentationImage img in ParentTile.Overlays)
                  //         {
                  //            if(img != null && img.Img != null)
                  //            {
                  //               if(!found)
                  //                  found = true;

                  //               //System.Drawing.RectangleF srcRect = new System.Drawing.RectangleF((float) (Xoff * (img.Img.Width / Ix)), (float) (Yoff * (img.Img.Height / Ix)), (img.Img.Width / Ix), (img.Img.Height / Ix));
                  //               //System.Drawing.Rectangle dst = new System.Drawing.Rectangle(Core.tileRect.X, Core.tileRect.Y, Core.tileRect.Width, Core.tileRect.Height);

                  //               //g.DrawImage(img.Img, new Rect(

                  //               //g.DrawImage(img.Img, dst, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, GraphicsUnit.Pixel, TileFlipXYAttributes);
                  //               //g.FillRectangle(SelectedAreaFill, dst);
                  //            }
                  //         }
                  //      }
                  //   }
                  //}

                  // add text if tile is missing
                  if(!found)
                  {
                     lock(Core.FailedLoads)
                     {
                        var lt = new LoadTask(tilePoint.PosXY, Core.Zoom);

                        if(Core.FailedLoads.ContainsKey(lt))
                        {
                           g.DrawRectangle(EmptytileBrush, EmptyTileBorders, new Rect(Core.tileRect.X, Core.tileRect.Y, Core.tileRect.Width, Core.tileRect.Height));

                           var ex = Core.FailedLoads[lt];
                           FormattedText TileText = new FormattedText("Exception: " + ex.Message, System.Globalization.CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, tileTypeface, 14, Brushes.Red);
                           TileText.MaxTextWidth = Core.tileRect.Width - 11;

                           g.DrawText(TileText, new System.Windows.Point(Core.tileRect.X + 11, Core.tileRect.Y + 11));

                           g.DrawText(EmptyTileText, new System.Windows.Point(Core.tileRect.X + Core.tileRect.Width / 2 - EmptyTileText.Width / 2, Core.tileRect.Y + Core.tileRect.Height / 2 - EmptyTileText.Height / 2));
                        }
                     }
                  }

                  if(ShowTileGridLines)
                  {
                     g.DrawRectangle(null, EmptyTileBorders, new Rect(Core.tileRect.X, Core.tileRect.Y, Core.tileRect.Width, Core.tileRect.Height));

                     if(tilePoint.PosXY == Core.centerTileXYLocation)
                     {
                        FormattedText TileText = new FormattedText("CENTER:" + tilePoint.ToString(), System.Globalization.CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, tileTypeface, 16, Brushes.Red);
                        g.DrawText(TileText, new System.Windows.Point(Core.tileRect.X + Core.tileRect.Width / 2 - EmptyTileText.Width / 2, Core.tileRect.Y + Core.tileRect.Height / 2 - TileText.Height / 2));
                     }
                     else
                     {
                        FormattedText TileText = new FormattedText("TILE: " + tilePoint.ToString(), System.Globalization.CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, tileTypeface, 16, Brushes.Red);
                        g.DrawText(TileText, new System.Windows.Point(Core.tileRect.X + Core.tileRect.Width / 2 - EmptyTileText.Width / 2, Core.tileRect.Y + Core.tileRect.Height / 2 - TileText.Height / 2));
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
         (int)size.Width, (int)size.Height, 96, 96, PixelFormats.Pbgra32);

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
      public virtual Path CreateRoutePath(List<System.Windows.Point> localPath)
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
            myPath.IsHitTestVisible = false;
         }
         return myPath;
      }

      /// <summary>
      /// creates path from list of points
      /// </summary>
      /// <param name="pl"></param>
      /// <returns></returns>
      public virtual Path CreatePolygonPath(List<System.Windows.Point> localPath)
      {
         // Create a StreamGeometry to use to specify myPath.
         StreamGeometry geometry = new StreamGeometry();

         using(StreamGeometryContext ctx = geometry.Open())
         {
            ctx.BeginFigure(localPath[0], true, true);

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

            myPath.Stroke = Brushes.MidnightBlue;
            myPath.StrokeThickness = 5;
            myPath.StrokeLineJoin = PenLineJoin.Round;
            myPath.StrokeStartLineCap = PenLineCap.Triangle;
            myPath.StrokeEndLineCap = PenLineCap.Square;

            myPath.Fill = Brushes.AliceBlue;

            myPath.Opacity = 0.6;
            myPath.IsHitTestVisible = false;
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
            Position = center;

            if(maxZoom > MaxZoom)
            {
               maxZoom = MaxZoom;
            }

            if(Core.Zoom != maxZoom)
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
            Overlays = ItemsSource.Cast<GMapMarker>().Where(p => p != null && p.ZIndex == ZIndex);
         }
         else
         {
            Overlays = ItemsSource.Cast<GMapMarker>();
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
            Core.DragOffset(new GPoint(x, y));

            InvalidateVisual(true);
            UpdateMarkersOffset();
         }
      }

      readonly RotateTransform rotationMatrix = new RotateTransform();
      GeneralTransform rotationMatrixInvert = new RotateTransform();

      /// <summary>
      /// updates rotation matrix
      /// </summary>
      void UpdateRotationMatrix()
      {
         System.Windows.Point center = new System.Windows.Point(ActualWidth / 2.0, ActualHeight / 2.0);

         rotationMatrix.Angle = -Bearing;
         rotationMatrix.CenterY = center.Y;
         rotationMatrix.CenterX = center.X;

         rotationMatrixInvert = rotationMatrix.Inverse;
      }

      /// <summary>
      /// returs true if map bearing is not zero
      /// </summary>         
      public bool IsRotated
      {
         get
         {
            return Core.IsRotated;
         }
      }

      /// <summary>
      /// bearing for rotation of the map
      /// </summary>
      [Category("GMap.NET")]
      public float Bearing
      {
         get
         {
            return Core.bearing;
         }
         set
         {
            if(Core.bearing != value)
            {
               bool resize = Core.bearing == 0;
               Core.bearing = value;

               UpdateRotationMatrix();

               if(value != 0 && value % 360 != 0)
               {
                  Core.IsRotated = true;

                  if(Core.tileRectBearing.Size == Core.tileRect.Size)
                  {
                     Core.tileRectBearing = Core.tileRect;
                     Core.tileRectBearing.Inflate(1, 1);
                  }
               }
               else
               {
                  Core.IsRotated = false;
                  Core.tileRectBearing = Core.tileRect;
               }

               if(resize)
               {
                  Core.OnMapSizeChanged((int)ActualWidth, (int)ActualHeight);
               }

               Core_OnMapZoomChanged();

               InvalidateVisual();
            }
         }
      }

      /// <summary>
      /// apply transformation if in rotation mode
      /// </summary>
      System.Windows.Point ApplyRotation(double x, double y)
      {
         System.Windows.Point ret = new System.Windows.Point(x, y);

         if(IsRotated)
         {
            ret = rotationMatrix.Transform(ret);
         }

         return ret;
      }

      /// <summary>
      /// apply transformation if in rotation mode
      /// </summary>
      System.Windows.Point ApplyRotationInversion(double x, double y)
      {
         System.Windows.Point ret = new System.Windows.Point(x, y);

         if(IsRotated)
         {
            ret = rotationMatrixInvert.Transform(ret);
         }

         return ret;
      }

      #region UserControl Events
      protected override void OnRender(DrawingContext drawingContext)
      {
         if(!Core.IsStarted)
            return;

         drawingContext.DrawRectangle(EmptyMapBackground, null, new Rect(RenderSize));

         if(IsRotated)
         {
            drawingContext.PushTransform(rotationMatrix);

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

            drawingContext.Pop();
         }
         else
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
         }

         // selection
         if(!SelectedArea.IsEmpty)
         {
            GPoint p1 = FromLatLngToLocal(SelectedArea.LocationTopLeft);
            GPoint p2 = FromLatLngToLocal(SelectedArea.LocationRightBottom);

            int x1 = p1.X;
            int y1 = p1.Y;
            int x2 = p2.X;
            int y2 = p2.Y;

            if(SelectionUseCircle)
            {
               drawingContext.DrawEllipse(SelectedAreaFill, SelectionPen, new System.Windows.Point(x1 + (x2 - x1) / 2, y1 + (y2 - y1) / 2), (x2 - x1) / 2, (y2 - y1) / 2);
            }
            else
            {
               drawingContext.DrawRoundedRectangle(SelectedAreaFill, SelectionPen, new Rect(x1, y1, x2 - x1, y2 - y1), 5, 5);
            }
         }

         if(ShowCenter)
         {
            drawingContext.DrawLine(CenterCrossPen, new System.Windows.Point((ActualWidth / 2) - 10, ActualHeight / 2), new System.Windows.Point((ActualWidth / 2) + 10, ActualHeight / 2));
            drawingContext.DrawLine(CenterCrossPen, new System.Windows.Point(ActualWidth / 2, (ActualHeight / 2) - 10), new System.Windows.Point(ActualWidth / 2, (ActualHeight / 2) + 10));
         }

         #region -- copyright --

         if(Copyright != null)
         {
            drawingContext.DrawText(Copyright, new System.Windows.Point(5, ActualHeight - Copyright.Height - 5));
         }

         #endregion

         base.OnRender(drawingContext);
      }

      public Pen CenterCrossPen = new Pen(Brushes.Red, 1);
      public bool ShowCenter = true;

      /// <summary>
      /// reverses MouseWheel zooming direction
      /// </summary>
      public bool InvertedMouseWheelZooming = false;

      /// <summary>
      /// lets you zoom by MouseWheel even when pointer is in area of marker
      /// </summary>
      public bool IgnoreMarkerOnMouseWheel = false;

      protected override void OnMouseWheel(MouseWheelEventArgs e)
      {
         base.OnMouseWheel(e);

         if((IsMouseDirectlyOver || IgnoreMarkerOnMouseWheel) && !Core.IsDragging)
         {
            System.Windows.Point p = e.GetPosition(this);
            //p = ApplyRotationInversion(p.X, p.Y);

            if(Core.mouseLastZoom.X != (int)p.X && Core.mouseLastZoom.Y != (int)p.Y)
            {
               if(MouseWheelZoomType == MouseWheelZoomType.MousePositionAndCenter)
               {
                  Core.currentPosition = FromLocalToLatLng((int)p.X, (int)p.Y);
               }
               else if(MouseWheelZoomType == MouseWheelZoomType.ViewCenter)
               {
                  Core.currentPosition = FromLocalToLatLng((int)ActualWidth / 2, (int)ActualHeight / 2);
               }
               else if(MouseWheelZoomType == MouseWheelZoomType.MousePositionWithoutCenter)
               {
                  Core.currentPosition = FromLocalToLatLng((int)p.X, (int)p.Y);
               }

               Core.mouseLastZoom.X = (int)p.X;
               Core.mouseLastZoom.Y = (int)p.Y;
            }

            // set mouse position to map center
            if(MouseWheelZoomType != MouseWheelZoomType.MousePositionWithoutCenter)
            {
               System.Windows.Point ps = PointToScreen(new System.Windows.Point(ActualWidth / 2, ActualHeight / 2));
               Stuff.SetCursorPos((int)ps.X, (int)ps.Y);
            }

            Core.MouseWheelZooming = true;

            if(e.Delta > 0)
            {
               if(!InvertedMouseWheelZooming)
               {
                  Zoom = ((int)Zoom) + 1;
               }
               else
               {
                  Zoom = ((int)(Zoom + 0.99)) - 1;
               }
            }
            else
            {
               if(InvertedMouseWheelZooming)
               {
                  Zoom = ((int)Zoom) + 1;
               }
               else
               {
                  Zoom = ((int)(Zoom + 0.99)) - 1;
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
            Point p = e.GetPosition(this);

            if(MapRenderTransform != null)
            {
               p = MapRenderTransform.Inverse.Transform(p);
            }

            p = ApplyRotationInversion(p.X, p.Y);

            Core.mouseDown.X = (int)p.X;
            Core.mouseDown.Y = (int)p.Y;

            InvalidateVisual();
         }
         else
         {
            if(!isSelected)
            {
               Point p = e.GetPosition(this);
               isSelected = true;
               SelectedArea = RectLatLng.Empty;
               selectionEnd = PointLatLng.Zero;
               selectionStart = FromLocalToLatLng((int)p.X, (int)p.Y);
            }
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
            if(isDragging)
            {
               Mouse.Capture(null);

               isDragging = false;
               Debug.WriteLine("IsDragging = " + isDragging);
               Cursor = cursorBefore;
            }
            Core.EndDrag();

            if(BoundsOfMap.HasValue && !BoundsOfMap.Value.Contains(Position))
            {
               if(Core.LastLocationInBounds.HasValue)
               {
                  Position = Core.LastLocationInBounds.Value;
               }
            }
         }
         else
         {
            if(!selectionEnd.IsZero && !selectionStart.IsZero)
            {
               if(!SelectedArea.IsEmpty && Keyboard.Modifiers == ModifierKeys.Shift)
               {
                  SetZoomToFitRect(SelectedArea);
               }
            }
            else
            {
               if(e.ChangedButton == DragButton)
               {
                  Core.mouseDown = GPoint.Empty;
               }
               InvalidateVisual();
            }
         }

         base.OnMouseUp(e);
      }

      Cursor cursorBefore = Cursors.Arrow;

      protected override void OnMouseMove(MouseEventArgs e)
      {
         if(!Core.IsDragging && !Core.mouseDown.IsEmpty)
         {
            Point p = e.GetPosition(this);

            if(MapRenderTransform != null)
            {
               p = MapRenderTransform.Inverse.Transform(p);
            }

            p = ApplyRotationInversion(p.X, p.Y);

            // cursor has moved beyond drag tolerance
            if(Math.Abs(p.X - Core.mouseDown.X) * 2 >= SystemParameters.MinimumHorizontalDragDistance || Math.Abs(p.Y - Core.mouseDown.Y) * 2 >= SystemParameters.MinimumVerticalDragDistance)
            {
               Core.BeginDrag(Core.mouseDown);
            }
         }

         if(Core.IsDragging)
         {
            if(!isDragging)
            {
               Mouse.Capture(this);

               isDragging = true;
               Debug.WriteLine("IsDragging = " + isDragging);

               cursorBefore = Cursor;
               Cursor = Cursors.SizeAll;
            }

            if(BoundsOfMap.HasValue && !BoundsOfMap.Value.Contains(Position))
            {
               // ...
            }
            else
            {
               Point p = e.GetPosition(this);

               if(MapRenderTransform != null)
               {
                  p = MapRenderTransform.Inverse.Transform(p);
               }

               p = ApplyRotationInversion(p.X, p.Y);

               Core.mouseCurrent.X = (int)p.X;
               Core.mouseCurrent.Y = (int)p.Y;
               {
                  Core.Drag(Core.mouseCurrent);
               }

               if(IsRotated)
               {
                  Core_OnMapZoomChanged();
               }
               else
               {
                  UpdateMarkersOffset();
               }
            }
            InvalidateVisual(true);
         }
         else
         {
            if(isSelected && !selectionStart.IsZero && (Keyboard.Modifiers == ModifierKeys.Shift || Keyboard.Modifiers == ModifierKeys.Alt))
            {
               System.Windows.Point p = e.GetPosition(this);
               selectionEnd = FromLocalToLatLng((int)p.X, (int)p.Y);
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

      protected override void OnStylusDown(StylusDownEventArgs e)
      {
         if(TouchEnabled && CanDragMap && !e.InAir)
         {
            Point p = e.GetPosition(this);

            if(MapRenderTransform != null)
            {
               p = MapRenderTransform.Inverse.Transform(p);
            }

            p = ApplyRotationInversion(p.X, p.Y);

            Core.mouseDown.X = (int)p.X;
            Core.mouseDown.Y = (int)p.Y;

            InvalidateVisual();
         }

         base.OnStylusDown(e);
      }

      protected override void OnStylusUp(StylusEventArgs e)
      {
         if(TouchEnabled)
         {
            if(isSelected)
            {
               isSelected = false;
            }

            if(Core.IsDragging)
            {
               if(isDragging)
               {
                  Mouse.Capture(null);

                  isDragging = false;
                  Debug.WriteLine("IsDragging = " + isDragging);
                  Cursor = cursorBefore;
               }
               Core.EndDrag();

               if(BoundsOfMap.HasValue && !BoundsOfMap.Value.Contains(Position))
               {
                  if(Core.LastLocationInBounds.HasValue)
                  {
                     Position = Core.LastLocationInBounds.Value;
                  }
               }
            }
            else
            {
               Core.mouseDown = GPoint.Empty;
               InvalidateVisual();
            }
         }
         base.OnStylusUp(e);
      }

      protected override void OnStylusMove(StylusEventArgs e)
      {
         if(TouchEnabled)
         {
            if(!Core.IsDragging && !Core.mouseDown.IsEmpty)
            {
               Point p = e.GetPosition(this);

               if(MapRenderTransform != null)
               {
                  p = MapRenderTransform.Inverse.Transform(p);
               }

               p = ApplyRotationInversion(p.X, p.Y);

               // cursor has moved beyond drag tolerance
               if(Math.Abs(p.X - Core.mouseDown.X) * 2 >= SystemParameters.MinimumHorizontalDragDistance || Math.Abs(p.Y - Core.mouseDown.Y) * 2 >= SystemParameters.MinimumVerticalDragDistance)
               {
                  Core.BeginDrag(Core.mouseDown);
               }
            }

            if(Core.IsDragging)
            {
               if(!isDragging)
               {
                  Mouse.Capture(this);

                  isDragging = true;
                  Debug.WriteLine("IsDragging = " + isDragging);

                  cursorBefore = Cursor;
                  Cursor = Cursors.SizeAll;
               }

               if(BoundsOfMap.HasValue && !BoundsOfMap.Value.Contains(Position))
               {
                  // ...
               }
               else
               {
                  Point p = e.GetPosition(this);

                  if(MapRenderTransform != null)
                  {
                     p = MapRenderTransform.Inverse.Transform(p);
                  }

                  p = ApplyRotationInversion(p.X, p.Y);

                  Core.mouseCurrent.X = (int)p.X;
                  Core.mouseCurrent.Y = (int)p.Y;
                  {
                     Core.Drag(Core.mouseCurrent);
                  }

                  if(IsRotated)
                  {
                     Core_OnMapZoomChanged();
                  }
                  else
                  {
                     UpdateMarkersOffset();
                  }
               }
               InvalidateVisual();
            }
         }

         base.OnStylusMove(e);
      }

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
      /// sets position using geocoder
      /// </summary>
      /// <param name="keys"></param>
      /// <returns></returns>
      public GeoCoderStatusCode SetCurrentPositionByKeywords(string keys)
      {
         GeoCoderStatusCode status = GeoCoderStatusCode.Unknow;

         GeocodingProvider gp = GMapProviders.GoogleMap as GeocodingProvider;
         if(gp != null)
         {
            var pt = gp.GetPoint(keys, out status);
            if(status == GeoCoderStatusCode.G_GEO_SUCCESS && pt.HasValue)
            {
               Position = pt.Value;
            }
         }

         return status;
      }

      public PointLatLng FromLocalToLatLng(int x, int y)
      {
         if(MapRenderTransform != null)
         {
            var tp = MapRenderTransform.Inverse.Transform(new System.Windows.Point(x, y));
            x = (int)tp.X;
            y = (int)tp.Y;
         }

         if(IsRotated)
         {
            var f = rotationMatrixInvert.Transform(new System.Windows.Point(x, y));

            x = (int)f.X;
            y = (int)f.Y;
         }

         return Core.FromLocalToLatLng(x, y);
      }

      public GPoint FromLatLngToLocal(PointLatLng point)
      {
         GPoint ret = Core.FromLatLngToLocal(point);

         if(MapRenderTransform != null)
         {
            var tp = MapRenderTransform.Transform(new System.Windows.Point(ret.X, ret.Y));
            ret.X = (int)tp.X;
            ret.Y = (int)tp.Y;
         }

         if(IsRotated)
         {
            var f = rotationMatrix.Transform(new System.Windows.Point(ret.X, ret.Y));

            ret.X = (int)f.X;
            ret.Y = (int)f.Y;
         }

         return ret;
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

      /// <summary>
      /// current coordinates of the map center
      /// </summary>
      [Browsable(false)]
      public PointLatLng Position
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
      public GPoint CurrentPositionGPixel
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

      bool isDragging = false;

      [Browsable(false)]
      public bool IsDragging
      {
         get
         {
            return isDragging;
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

      public event PositionChanged OnPositionChanged
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
