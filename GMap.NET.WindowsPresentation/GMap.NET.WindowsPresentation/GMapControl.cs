
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
   using GMap.NET.Projections;

   /// <summary>
   /// GMap.NET control for Windows Presentation
   /// </summary>
   public partial class GMapControl : ItemsControl, Interface, IDisposable
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
      [Browsable(false)]
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
               viewarea = map.ViewArea;
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
               if(viewarea != RectLatLng.Empty && viewarea != map.ViewArea)
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

      private ScaleModes scaleMode = ScaleModes.Integer;

      /// <summary>
      /// Specifies, if a floating map scale is displayed using a 
      /// stretched, or a narrowed map.
      /// If <code>ScaleMode</code> is <code>ScaleDown</code>,
      /// then a scale of 12.3 is displayed using a map zoom level of 13
      /// resized to the lower level. If the parameter is <code>ScaleUp</code> ,
      /// then the same scale is displayed using a zoom level of 12 with an
      /// enlarged scale. If the value is <code>Dynamic</code>, then until a
      /// remainder of 0.25 <code>ScaleUp</code> is applied, for bigger
      /// remainders <code>ScaleDown</code>.
      /// </summary>
      [Category("GMap.NET")]
      [Description("map scale type")]
      public ScaleModes ScaleMode
      {
         get
         {
            return scaleMode;
         }
         set
         {
            scaleMode = value;
            InvalidateVisual();
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
            if(map.ScaleMode != ScaleModes.Integer && remainder != 0 && map.ActualWidth > 0)
            {
               bool scaleDown;

               switch(map.ScaleMode)
               {
                  case ScaleModes.ScaleDown:
                  scaleDown = true;
                  break;

                  case ScaleModes.Dynamic:
                  scaleDown = remainder > 0.25;
                  break;

                  default:
                  scaleDown = false;
                  break;
               }

               if(scaleDown)
                  remainder--;

               double scaleValue = Math.Pow(2d, remainder);
               {
                  if(map.MapScaleTransform == null)
                  {
                     map.MapScaleTransform = map.lastScaleTransform;
                  }
                  map.MapScaleTransform.ScaleX = scaleValue;
                  map.MapScaleTransform.ScaleY = scaleValue;

                  map.Core.scaleX = 1 / scaleValue;
                  map.Core.scaleY = 1 / scaleValue;

                  map.MapScaleTransform.CenterX = map.ActualWidth / 2;
                  map.MapScaleTransform.CenterY = map.ActualHeight / 2;
               }

               map.Core.Zoom = Convert.ToInt32(scaleDown ? Math.Ceiling(value) : value - remainder);
            }
            else
            {
               map.MapScaleTransform = null;
               map.Core.scaleX = 1;
               map.Core.scaleY = 1;
               map.Core.Zoom = (int)Math.Floor(value);
            }

            if(map.IsLoaded)
            {
               map.ForceUpdateOverlays();
               map.InvalidateVisual(true);
            }
         }
      }

      readonly ScaleTransform lastScaleTransform = new ScaleTransform();

      #endregion

      readonly Core Core = new Core();
      //GRect region;
      delegate void MethodInvoker();
      PointLatLng selectionStart;
      PointLatLng selectionEnd;
      Typeface tileTypeface = new Typeface("Arial");
      bool showTileGridLines = false;

      FormattedText Copyright;

      /// <summary>
      /// enables filling empty tiles using lower level images
      /// </summary>
      [Browsable(false)]
      public bool FillEmptyTiles
      {
         get
         {
            return Core.fillEmptyTiles;
         }
         set
         {
            Core.fillEmptyTiles = value;
         }
      }

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
      public bool TouchEnabled = true;

      /// <summary>
      /// map boundaries
      /// </summary>
      public RectLatLng? BoundsOfMap = null;

      /// <summary>
      /// occurs when mouse selection is changed
      /// </summary>        
      public event SelectionChange OnSelectionChange;

      /// <summary>
      /// list of markers
      /// </summary>
      public readonly ObservableCollection<GMapMarker> Markers = new ObservableCollection<GMapMarker>();

      /// <summary>
      /// current markers overlay offset
      /// </summary>
      internal readonly TranslateTransform MapTranslateTransform = new TranslateTransform();
      internal readonly TranslateTransform MapOverlayTranslateTransform = new TranslateTransform();

      internal ScaleTransform MapScaleTransform = new ScaleTransform();
      internal RotateTransform MapRotateTransform = new RotateTransform();

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

      static DataTemplate DataTemplateInstance;
      static ItemsPanelTemplate ItemsPanelTemplateInstance;
      static Style StyleInstance;

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

            if(DataTemplateInstance == null)
            {
               DataTemplateInstance = new DataTemplate(typeof(GMapMarker));
               {
                  FrameworkElementFactory fef = new FrameworkElementFactory(typeof(ContentPresenter));
                  fef.SetBinding(ContentPresenter.ContentProperty, new Binding("Shape"));
                  DataTemplateInstance.VisualTree = fef;
               }
            }
            ItemTemplate = DataTemplateInstance;

            if(ItemsPanelTemplateInstance == null)
            {
               var factoryPanel = new FrameworkElementFactory(typeof(Canvas));
               {
                  factoryPanel.SetValue(Canvas.IsItemsHostProperty, true);

                  ItemsPanelTemplateInstance = new ItemsPanelTemplate();
                  {
                     ItemsPanelTemplateInstance.VisualTree = factoryPanel;
                  }
               }
            }
            ItemsPanel = ItemsPanelTemplateInstance;

            if(StyleInstance == null)
            {
               StyleInstance = new Style();
               {
                  StyleInstance.Setters.Add(new Setter(Canvas.LeftProperty, new Binding("LocalPositionX")));
                  StyleInstance.Setters.Add(new Setter(Canvas.TopProperty, new Binding("LocalPositionY")));
                  StyleInstance.Setters.Add(new Setter(Canvas.ZIndexProperty, new Binding("ZIndex")));
               }
            }
            ItemContainerStyle = StyleInstance;
            #endregion

            ClipToBounds = true;
            SnapsToDevicePixels = true;

            Core.SystemType = "WindowsPresentation";

            Core.RenderMode = GMap.NET.RenderMode.WPF;

            Core.OnMapZoomChanged += new MapZoomChanged(ForceUpdateOverlays);
            Loaded += new RoutedEventHandler(GMapControl_Loaded);
            Dispatcher.ShutdownStarted += new EventHandler(Dispatcher_ShutdownStarted);
            SizeChanged += new SizeChangedEventHandler(GMapControl_SizeChanged);

            // by default its internal property, feel free to use your own
            if(ItemsSource == null)
            {
               ItemsSource = Markers;
            }

            Core.Zoom = (int)((double)ZoomProperty.DefaultMetadata.DefaultValue);
         }
      }

      static GMapControl()
      {
         GMapImageProxy.Enable();
#if !PocketPC
         GMaps.Instance.SQLitePing();
#endif
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
         if(Core.Refresh != null)
         {
            Core.Refresh.Set();
         }
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
         base.OnItemsChanged(e);
         
         if(e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
         {
             ForceUpdateOverlays(e.NewItems);
         }
         else
         {
             InvalidateVisual();
         }
      }

      /// <summary>
      /// inits core system
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void GMapControl_Loaded(object sender, RoutedEventArgs e)
      {
         if(!Core.IsStarted)
         {
            if(lazyEvents)
            {
               lazyEvents = false;

               if(lazySetZoomToFitRect.HasValue)
               {
                  SetZoomToFitRect(lazySetZoomToFitRect.Value);
                  lazySetZoomToFitRect = null;
               }
            }
            Core.OnMapOpen().ProgressChanged += new ProgressChangedEventHandler(invalidatorEngage);
            ForceUpdateOverlays();

            if(Application.Current != null)
            {
               loadedApp = Application.Current;

               loadedApp.Dispatcher.Invoke(DispatcherPriority.ApplicationIdle,
                  new Action(delegate()
                  {
                     loadedApp.SessionEnding += new SessionEndingCancelEventHandler(Current_SessionEnding);
                  }
                  ));
            }
         }
      }

      Application loadedApp;

      void Current_SessionEnding(object sender, SessionEndingCancelEventArgs e)
      {
         GMaps.Instance.CancelTileCaching();
      }

      void Dispatcher_ShutdownStarted(object sender, EventArgs e)
      {
         Dispose();
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
         //region = new GRect(-50, -50, (int)constraint.Width + 100, (int)constraint.Height + 100);

         Core.OnMapSizeChanged((int)constraint.Width, (int)constraint.Height);

         if(Core.IsStarted)
         {
            if(IsRotated)
            {
               UpdateRotationMatrix();
            }

            ForceUpdateOverlays();
         }
      }
      
      void ForceUpdateOverlays()
      {
          ForceUpdateOverlays(ItemsSource);          
      }

      void ForceUpdateOverlays(System.Collections.IEnumerable items)
      {
         using(Dispatcher.DisableProcessing())
         {
            UpdateMarkersOffset();

            foreach(GMapMarker i in items)
            {
               if(i != null)
               {
                  i.ForceUpdateLocalPosition(this);

                  if(i is IShapable)
                  {
                     (i as IShapable).RegenerateShape(this);
                  }
               }
            }
         }
         InvalidateVisual();
      }

      /// <summary>
      /// updates markers overlay offset
      /// </summary>
      void UpdateMarkersOffset()
      {
         if(MapCanvas != null)
         {
            if(MapScaleTransform != null)
            {
               var tp = MapScaleTransform.Transform(new System.Windows.Point(Core.renderOffset.X, Core.renderOffset.Y));
               MapOverlayTranslateTransform.X = tp.X;
               MapOverlayTranslateTransform.Y = tp.Y;

               // map is scaled already
               MapTranslateTransform.X = Core.renderOffset.X;
               MapTranslateTransform.Y = Core.renderOffset.Y;
            }
            else
            {
               MapTranslateTransform.X = Core.renderOffset.X;
               MapTranslateTransform.Y = Core.renderOffset.Y;

               MapOverlayTranslateTransform.X = MapTranslateTransform.X;
               MapOverlayTranslateTransform.Y = MapTranslateTransform.Y;
            }
         }
      }

      public Brush EmptyMapBackground = Brushes.WhiteSmoke;

      /// <summary>
      /// render map in WPF
      /// </summary>
      /// <param name="g"></param>
      void DrawMap(DrawingContext g)
      {
         if(MapProvider == EmptyProvider.Instance || MapProvider == null)
         {
            return;
         }

         Core.tileDrawingListLock.AcquireReaderLock();
         Core.Matrix.EnterReadLock();
         try
         {
            foreach(var tilePoint in Core.tileDrawingList)
            {
               Core.tileRect.Location = tilePoint.PosPixel;
               Core.tileRect.OffsetNegative(Core.compensationOffset);

               //if(region.IntersectsWith(Core.tileRect) || IsRotated)
               {
                  bool found = false;

                  Tile t = Core.Matrix.GetTileWithNoLock(Core.Zoom, tilePoint.PosXY);
                  if(t.NotEmpty)
                  {
                     foreach(GMapImage img in t.Overlays)
                     {
                        if(img != null && img.Img != null)
                        {
                           if(!found)
                              found = true;

                           var imgRect = new Rect(Core.tileRect.X + 0.6, Core.tileRect.Y + 0.6, Core.tileRect.Width + 0.6, Core.tileRect.Height + 0.6);
                           if(!img.IsParent)
                           {
                              g.DrawImage(img.Img, imgRect);
                           }
                           else
                           {
                              // TODO: move calculations to loader thread
                              var geometry = new RectangleGeometry(imgRect);
                              var parentImgRect = new Rect(Core.tileRect.X - Core.tileRect.Width * img.Xoff + 0.6, Core.tileRect.Y - Core.tileRect.Height * img.Yoff + 0.6, Core.tileRect.Width * img.Ix + 0.6, Core.tileRect.Height * img.Ix + 0.6);

                              g.PushClip(geometry);
                              g.DrawImage(img.Img, parentImgRect);
                              g.Pop();
                              geometry = null;
                           }
                        }
                     }
                  }
                  else if(FillEmptyTiles && MapProvider.Projection is MercatorProjection)
                  {
                     #region -- fill empty tiles --
                     int zoomOffset = 1;
                     Tile parentTile = Tile.Empty;
                     long Ix = 0;

                     while(!parentTile.NotEmpty && zoomOffset < Core.Zoom && zoomOffset <= LevelsKeepInMemmory)
                     {
                        Ix = (long)Math.Pow(2, zoomOffset);
                        parentTile = Core.Matrix.GetTileWithNoLock(Core.Zoom - zoomOffset++, new GMap.NET.GPoint((int)(tilePoint.PosXY.X / Ix), (int)(tilePoint.PosXY.Y / Ix)));
                     }

                     if(parentTile.NotEmpty)
                     {
                        long Xoff = Math.Abs(tilePoint.PosXY.X - (parentTile.Pos.X * Ix));
                        long Yoff = Math.Abs(tilePoint.PosXY.Y - (parentTile.Pos.Y * Ix));

                        var geometry = new RectangleGeometry(new Rect(Core.tileRect.X + 0.6, Core.tileRect.Y + 0.6, Core.tileRect.Width + 0.6, Core.tileRect.Height + 0.6));
                        var parentImgRect = new Rect(Core.tileRect.X - Core.tileRect.Width * Xoff + 0.6, Core.tileRect.Y - Core.tileRect.Height * Yoff + 0.6, Core.tileRect.Width * Ix + 0.6, Core.tileRect.Height * Ix + 0.6);

                        // render tile 
                        {
                           foreach(GMapImage img in parentTile.Overlays)
                           {
                              if(img != null && img.Img != null && !img.IsParent)
                              {
                                 if(!found)
                                    found = true;

                                 g.PushClip(geometry);
                                 g.DrawImage(img.Img, parentImgRect);
                                 g.DrawRectangle(SelectedAreaFill, null, geometry.Bounds);
                                 g.Pop();
                              }
                           }
                        }

                        geometry = null;
                     }
                     #endregion
                  }

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
                        TileText.MaxTextWidth = Core.tileRect.Width;
                        g.DrawText(TileText, new System.Windows.Point(Core.tileRect.X + Core.tileRect.Width / 2 - EmptyTileText.Width / 2, Core.tileRect.Y + Core.tileRect.Height / 2 - TileText.Height / 2));
                     }
                     else
                     {
                        FormattedText TileText = new FormattedText("TILE: " + tilePoint.ToString(), System.Globalization.CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, tileTypeface, 16, Brushes.Red);
                        TileText.MaxTextWidth = Core.tileRect.Width;
                        g.DrawText(TileText, new System.Windows.Point(Core.tileRect.X + Core.tileRect.Width / 2 - EmptyTileText.Width / 2, Core.tileRect.Y + Core.tileRect.Height / 2 - TileText.Height / 2));
                     }
                  }
               }
            }
         }
         finally
         {
            Core.Matrix.LeaveReadLock();
            Core.tileDrawingListLock.ReleaseReaderLock();
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
         Size size = new Size(obj.ActualWidth, obj.ActualHeight);

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
      /// creates path from list of points, for performance set addBlurEffect to false
      /// </summary>
      /// <param name="pl"></param>
      /// <returns></returns>
      public virtual Path CreateRoutePath(List<Point> localPath)
      {
         return CreateRoutePath(localPath, true);
      }

      /// <summary>
      /// creates path from list of points, for performance set addBlurEffect to false
      /// </summary>
      /// <param name="pl"></param>
      /// <returns></returns>
      public virtual Path CreateRoutePath(List<Point> localPath, bool addBlurEffect)
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

            if (addBlurEffect)
            {
                BlurEffect ef = new BlurEffect();
                {
                    ef.KernelType = KernelType.Gaussian;
                    ef.Radius = 3.0;
                    ef.RenderingBias = RenderingBias.Performance;
                }

                myPath.Effect = ef;
            }

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
      /// creates path from list of points, for performance set addBlurEffect to false
      /// </summary>
      /// <param name="pl"></param>
      /// <returns></returns>
      public virtual Path CreatePolygonPath(List<Point> localPath)
      {
         return CreatePolygonPath(localPath, false);
      }

      /// <summary>
      /// creates path from list of points, for performance set addBlurEffect to false
      /// </summary>
      /// <param name="pl"></param>
      /// <returns></returns>
      public virtual Path CreatePolygonPath(List<Point> localPath, bool addBlurEffect)
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

            if (addBlurEffect)
            {
                BlurEffect ef = new BlurEffect();
                {
                    ef.KernelType = KernelType.Gaussian;
                    ef.Radius = 3.0;
                    ef.RenderingBias = RenderingBias.Performance;
                }

                myPath.Effect = ef;
            }

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
         if(lazyEvents)
         {
            lazySetZoomToFitRect = rect;
         }
         else
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
         }
         return false;
      }

      RectLatLng? lazySetZoomToFitRect = null;
      bool lazyEvents = true;

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
               if(m.Shape != null && m.Shape.Visibility == System.Windows.Visibility.Visible)
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
            if(IsRotated)
            {
               Point p = new Point(x, y);
               p = rotationMatrixInvert.Transform(p);
               x = (int)p.X;
               y = (int)p.Y;

               Core.DragOffset(new GPoint(x, y));

               ForceUpdateOverlays();
            }
            else
            {
               Core.DragOffset(new GPoint(x, y));

               UpdateMarkersOffset();
               InvalidateVisual(true);
            }
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

               if(Core.IsStarted)
               {
                  ForceUpdateOverlays();
               }
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

            if(MapScaleTransform != null)
            {
               drawingContext.PushTransform(MapScaleTransform); 
               drawingContext.PushTransform(MapTranslateTransform);
               {
                  DrawMap(drawingContext);
               }
               drawingContext.Pop();
               drawingContext.Pop();
            }
            else
            {
               drawingContext.PushTransform(MapTranslateTransform);
               {
                  DrawMap(drawingContext);
               }
               drawingContext.Pop();
            }

            drawingContext.Pop();
         }
         else
         {
            if(MapScaleTransform != null)
            {
               drawingContext.PushTransform(MapScaleTransform);
               drawingContext.PushTransform(MapTranslateTransform);
               {
                  DrawMap(drawingContext);

#if DEBUG
                  drawingContext.DrawLine(VirtualCenterCrossPen, new Point(-20, 0), new Point(20, 0));
                  drawingContext.DrawLine(VirtualCenterCrossPen, new Point(0, -20), new Point(0, 20));
#endif
               }
               drawingContext.Pop();
               drawingContext.Pop();
            }
            else
            {
               drawingContext.PushTransform(MapTranslateTransform);
               {
                  DrawMap(drawingContext);
#if DEBUG
                  drawingContext.DrawLine(VirtualCenterCrossPen, new Point(-20, 0), new Point(20, 0));
                  drawingContext.DrawLine(VirtualCenterCrossPen, new Point(0, -20), new Point(0, 20));
#endif
               }
               drawingContext.Pop();
            }
         }

         // selection
         if(!SelectedArea.IsEmpty)
         {
            GPoint p1 = FromLatLngToLocal(SelectedArea.LocationTopLeft);
            GPoint p2 = FromLatLngToLocal(SelectedArea.LocationRightBottom);

            long x1 = p1.X;
            long y1 = p1.Y;
            long x2 = p2.X;
            long y2 = p2.Y;

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
            drawingContext.DrawLine(CenterCrossPen, new System.Windows.Point((ActualWidth / 2) - 5, ActualHeight / 2), new System.Windows.Point((ActualWidth / 2) + 5, ActualHeight / 2));
            drawingContext.DrawLine(CenterCrossPen, new System.Windows.Point(ActualWidth / 2, (ActualHeight / 2) - 5), new System.Windows.Point(ActualWidth / 2, (ActualHeight / 2) + 5));
         }

         if(renderHelperLine)
         {
            var p = Mouse.GetPosition(this);

            drawingContext.DrawLine(HelperLinePen, new Point(p.X, 0), new Point(p.X, ActualHeight));
            drawingContext.DrawLine(HelperLinePen, new Point(0, p.Y), new Point(ActualWidth, p.Y));
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

#if DEBUG
      readonly Pen VirtualCenterCrossPen = new Pen(Brushes.Blue, 1);
#endif

      HelperLineOptions helperLineOption = HelperLineOptions.DontShow;

      /// <summary>
      /// draw lines at the mouse pointer position
      /// </summary>
      [Browsable(false)]
      public HelperLineOptions HelperLineOption
      {
         get
         {
            return helperLineOption;
         }
         set
         {
            helperLineOption = value;
            renderHelperLine = (helperLineOption == HelperLineOptions.ShowAlways);
            if(Core.IsStarted)
            {
               InvalidateVisual();
            }
         }
      }

      public Pen HelperLinePen = new Pen(Brushes.Blue, 1);
      bool renderHelperLine = false;

      protected override void OnKeyUp(KeyEventArgs e)
      {
         if(HelperLineOption == HelperLineOptions.ShowOnModifierKey)
         {
            renderHelperLine = !(e.IsUp && (e.Key == Key.LeftShift || e.SystemKey == Key.LeftAlt));
            if(!renderHelperLine)
            {
               InvalidateVisual();
            }
         }
         base.OnKeyUp(e);
      }

      protected override void OnKeyDown(KeyEventArgs e)
      {
         if(HelperLineOption == HelperLineOptions.ShowOnModifierKey)
         {
            renderHelperLine = e.IsDown && (e.Key == Key.LeftShift || e.SystemKey == Key.LeftAlt);
            if(renderHelperLine)
            {
               InvalidateVisual();
            }
         }
         base.OnKeyDown(e);
      }

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

            if(MapScaleTransform != null)
            {
               p = MapScaleTransform.Inverse.Transform(p);
            }

            p = ApplyRotationInversion(p.X, p.Y);

            if(Core.mouseLastZoom.X != (int)p.X && Core.mouseLastZoom.Y != (int)p.Y)
            {
               if(MouseWheelZoomType == MouseWheelZoomType.MousePositionAndCenter)
               {
                  Core.position = FromLocalToLatLng((int)p.X, (int)p.Y);
               }
               else if(MouseWheelZoomType == MouseWheelZoomType.ViewCenter)
               {
                  Core.position = FromLocalToLatLng((int)ActualWidth / 2, (int)ActualHeight / 2);
               }
               else if(MouseWheelZoomType == MouseWheelZoomType.MousePositionWithoutCenter)
               {
                  Core.position = FromLocalToLatLng((int)p.X, (int)p.Y);
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
         if(CanDragMap && e.ChangedButton == DragButton)
         {
            Point p = e.GetPosition(this);

            if(MapScaleTransform != null)
            {
               p = MapScaleTransform.Inverse.Transform(p);
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
               selectionEnd = PointLatLng.Empty;
               selectionStart = FromLocalToLatLng((int)p.X, (int)p.Y);
            }
         }
         base.OnMouseDown(e);
      }

      int onMouseUpTimestamp = 0;

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
               onMouseUpTimestamp = e.Timestamp & Int32.MaxValue;
               isDragging = false;
               Debug.WriteLine("IsDragging = " + isDragging);
               Cursor = cursorBefore;
               Mouse.Capture(null);
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
            if(e.ChangedButton == DragButton)
            {
               Core.mouseDown = GPoint.Empty;
            }

            if(!selectionEnd.IsEmpty && !selectionStart.IsEmpty)
            {
               bool zoomtofit = false;

               if(!SelectedArea.IsEmpty && Keyboard.Modifiers == ModifierKeys.Shift)
               {
                  zoomtofit = SetZoomToFitRect(SelectedArea);
               }

               if(OnSelectionChange != null)
               {
                  OnSelectionChange(SelectedArea, zoomtofit);
               }
            }
            else
            {
               InvalidateVisual();
            }
         }

         base.OnMouseUp(e);
      }

      Cursor cursorBefore = Cursors.Arrow;

      protected override void OnMouseMove(MouseEventArgs e)
      {
         // wpf generates to many events if mouse is over some visual
         // and OnMouseUp is fired, wtf, anyway...
         // http://greatmaps.codeplex.com/workitem/16013
         if((e.Timestamp & Int32.MaxValue) - onMouseUpTimestamp < 55)
         {
            Debug.WriteLine("OnMouseMove skipped: " + ((e.Timestamp & Int32.MaxValue) - onMouseUpTimestamp) + "ms");
            return;
         }

         if(!Core.IsDragging && !Core.mouseDown.IsEmpty)
         {
            Point p = e.GetPosition(this);

            if(MapScaleTransform != null)
            {
               p = MapScaleTransform.Inverse.Transform(p);
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
               isDragging = true;
               Debug.WriteLine("IsDragging = " + isDragging);
               cursorBefore = Cursor;
               Cursor = Cursors.SizeAll;
               Mouse.Capture(this);
            }

            if(BoundsOfMap.HasValue && !BoundsOfMap.Value.Contains(Position))
            {
               // ...
            }
            else
            {
               Point p = e.GetPosition(this);

               if(MapScaleTransform != null)
               {
                  p = MapScaleTransform.Inverse.Transform(p);
               }

               p = ApplyRotationInversion(p.X, p.Y);

               Core.mouseCurrent.X = (int)p.X;
               Core.mouseCurrent.Y = (int)p.Y;
               {
                  Core.Drag(Core.mouseCurrent);
               }

               if(IsRotated || scaleMode != ScaleModes.Integer)
               {
                  ForceUpdateOverlays();
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
            if(isSelected && !selectionStart.IsEmpty && (Keyboard.Modifiers == ModifierKeys.Shift || Keyboard.Modifiers == ModifierKeys.Alt || DisableAltForSelection))
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

            if(renderHelperLine)
            {
               InvalidateVisual(true);
            }
         }

         base.OnMouseMove(e);
      }

      /// <summary>
      /// if true, selects area just by holding mouse and moving
      /// </summary>
      public bool DisableAltForSelection = false;

      protected override void OnStylusDown(StylusDownEventArgs e)
      {
         if(TouchEnabled && CanDragMap && !e.InAir)
         {
            Point p = e.GetPosition(this);

            if(MapScaleTransform != null)
            {
               p = MapScaleTransform.Inverse.Transform(p);
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
                  onMouseUpTimestamp = e.Timestamp & Int32.MaxValue;
                  isDragging = false;
                  Debug.WriteLine("IsDragging = " + isDragging);
                  Cursor = cursorBefore;
                  Mouse.Capture(null);
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
            // wpf generates to many events if mouse is over some visual
            // and OnMouseUp is fired, wtf, anyway...
            // http://greatmaps.codeplex.com/workitem/16013
            if((e.Timestamp & Int32.MaxValue) - onMouseUpTimestamp < 55)
            {
               Debug.WriteLine("OnMouseMove skipped: " + ((e.Timestamp & Int32.MaxValue) - onMouseUpTimestamp) + "ms");
               return;
            }

            if(!Core.IsDragging && !Core.mouseDown.IsEmpty)
            {
               Point p = e.GetPosition(this);

               if(MapScaleTransform != null)
               {
                  p = MapScaleTransform.Inverse.Transform(p);
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
                  isDragging = true;
                  Debug.WriteLine("IsDragging = " + isDragging);
                  cursorBefore = Cursor;
                  Cursor = Cursors.SizeAll;
                  Mouse.Capture(this);
               }

               if(BoundsOfMap.HasValue && !BoundsOfMap.Value.Contains(Position))
               {
                  // ...
               }
               else
               {
                  Point p = e.GetPosition(this);

                  if(MapScaleTransform != null)
                  {
                     p = MapScaleTransform.Inverse.Transform(p);
                  }

                  p = ApplyRotationInversion(p.X, p.Y);

                  Core.mouseCurrent.X = (int)p.X;
                  Core.mouseCurrent.Y = (int)p.Y;
                  {
                     Core.Drag(Core.mouseCurrent);
                  }

                  if(IsRotated)
                  {
                     ForceUpdateOverlays();
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
      public GeoCoderStatusCode SetPositionByKeywords(string keys)
      {
         GeoCoderStatusCode status = GeoCoderStatusCode.Unknow;

         GeocodingProvider gp = MapProvider as GeocodingProvider;
         if(gp == null)
         {
            gp = GMapProviders.OpenStreetMap as GeocodingProvider;
         }

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
         if(MapScaleTransform != null)
         {
            var tp = MapScaleTransform.Inverse.Transform(new System.Windows.Point(x, y));
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

         if(MapScaleTransform != null)
         {
            var tp = MapScaleTransform.Transform(new System.Windows.Point(ret.X, ret.Y));
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
         var dlg = new Microsoft.Win32.SaveFileDialog();
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
         var dlg = new Microsoft.Win32.OpenFileDialog();
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
            return Core.Position;
         }
         set
         {
            Core.Position = value;

            if(Core.IsStarted)
            {
               ForceUpdateOverlays();
            }
         }
      }

      [Browsable(false)]
      public GPoint PositionPixel
      {
         get
         {
            return Core.PositionPixel;
         }
      }

      [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
      [Browsable(false)]
      public string CacheLocation
      {
         get
         {
            return CacheLocator.Location;
         }
         set
         {
            CacheLocator.Location = value;
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
      public RectLatLng ViewArea
      {
         get
         {
             if(!IsRotated)
             {
                 return Core.ViewArea;
             }
             else if(Core.Provider.Projection != null)
             {
                 var p = FromLocalToLatLng(0, 0);
                 var p2 = FromLocalToLatLng((int)Width, (int)Height);
    
                 return RectLatLng.FromLTRB(p.Lng, p.Lat, p2.Lng, p2.Lat);
             }
             return RectLatLng.Empty;
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

      #region IDisposable Members

      public virtual void Dispose()
      {
         if(Core.IsStarted)
         {
            Core.OnMapZoomChanged -= new MapZoomChanged(ForceUpdateOverlays);
            Loaded -= new RoutedEventHandler(GMapControl_Loaded);
            Dispatcher.ShutdownStarted -= new EventHandler(Dispatcher_ShutdownStarted);
            SizeChanged -= new SizeChangedEventHandler(GMapControl_SizeChanged);
            if(loadedApp != null)
            {
               loadedApp.SessionEnding -= new SessionEndingCancelEventHandler(Current_SessionEnding);
            }
            Core.OnMapClose();
         }
      }

      #endregion
   }

   public enum HelperLineOptions
   {
      DontShow = 0,
      ShowAlways = 1,
      ShowOnModifierKey = 2
   }

   public enum ScaleModes
   {
      /// <summary>
      /// no scaling
      /// </summary>
      Integer,

      /// <summary>
      /// scales to fractional level using a stretched tiles, any issues -> http://greatmaps.codeplex.com/workitem/16046
      /// </summary>
      ScaleUp,

      /// <summary>
      /// scales to fractional level using a narrowed tiles, any issues -> http://greatmaps.codeplex.com/workitem/16046
      /// </summary>
      ScaleDown,

      /// <summary>
      /// scales to fractional level using a combination both stretched and narrowed tiles, any issues -> http://greatmaps.codeplex.com/workitem/16046
      /// </summary>
      Dynamic
   }

   public delegate void SelectionChange(RectLatLng Selection, bool ZoomToFit);
}
