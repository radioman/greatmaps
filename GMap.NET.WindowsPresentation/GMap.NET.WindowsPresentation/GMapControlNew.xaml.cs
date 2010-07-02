
namespace GMap.NET.WindowsPresentation
{
   using System;
   using System.Collections.Generic;
   using System.Collections.ObjectModel;
   using System.ComponentModel;
   using System.Diagnostics;
   using System.Globalization;
   using System.Linq;
   using System.Windows;
   using System.Windows.Controls;
   using System.Windows.Input;
   using System.Windows.Media;
   using System.Windows.Media.Animation;
   using System.Windows.Media.Effects;
   using System.Windows.Media.Imaging;
   using System.Windows.Shapes;
   using System.Windows.Threading;
   using GMap.NET;
   using GMap.NET.Internals;

   /// <summary>
   /// GMap.NET control for Windows Presentation
   /// </summary>
   public partial class GMapControlNew : UserControl, IGControl
   {
      readonly Core Core = new Core();
      GMap.NET.Rectangle region = new GMap.NET.Rectangle();
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

      /// <summary>
      /// max zoom
      /// </summary>
      public int MaxZoom = 2;

      /// <summary>
      /// min zoom
      /// </summary>
      public int MinZoom = 2;

      /// <summary>
      /// map zooming type for mouse wheel
      /// </summary>
      public MouseWheelZoomType MouseWheelZoomType = MouseWheelZoomType.MousePositionAndCenter;

      /// <summary>
      /// center mouse OnMouseWheel
      /// </summary>
      public bool CenterPositionOnMouseWheel = true;

      /// <summary>
      /// map dragg button
      /// </summary>
      public MouseButton DragButton = MouseButton.Right;

      /// <summary>
      /// zoom increment on mouse wheel
      /// </summary>
      public double ZoomIncrement = 1.0;

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

               double remainder = (double) System.Decimal.Remainder((Decimal) value, (Decimal) 1);
               if(remainder != 0)
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
               // if(ObjectsLayer.VisualChildrenCount > 0)
               {
                  Border border = VisualTreeHelper.GetChild(ObjectsLayer, 0) as Border;
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

      public GMapControlNew()
      {
         InitializeComponent();

         if(!DesignModeInConstruct)
         {
            ObjectsLayer.ItemsSource = Markers;

            // removes white lines between tiles!
            SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Unspecified);

            // set image proxy
            Manager.ImageProxy = new WindowsPresentationImageProxy();

            //Core.RenderMode = GMap.NET.RenderMode.WPF;
            //Core.OnNeedInvalidation += new NeedInvalidation(Core_OnNeedInvalidation);
            //Core.OnMapZoomChanged += new MapZoomChanged(Core_OnMapZoomChanged);

            Loaded += new RoutedEventHandler(GMapControl_Loaded);
            Unloaded += new RoutedEventHandler(GMapControl_Unloaded);

            googleCopyright = new FormattedText(Core.googleCopyright, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface("GenericSansSerif"), 9, Brushes.Navy);
            yahooMapCopyright = new FormattedText(Core.yahooMapCopyright, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface("GenericSansSerif"), 9, Brushes.Navy);
            virtualEarthCopyright = new FormattedText(Core.virtualEarthCopyright, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface("GenericSansSerif"), 9, Brushes.Navy);
            openStreetMapCopyright = new FormattedText(Core.openStreetMapCopyright, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface("GenericSansSerif"), 9, Brushes.Navy);
            arcGisMapCopyright = new FormattedText(Core.arcGisCopyright, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface("GenericSansSerif"), 9, Brushes.Navy);

            MapType = MapType.GoogleMap;

            OpacityAnimation = CreateOpacityAnimation(1);
            ZoomAnimation = CreateZoomAnimation(2);
            MoveAnimation = CreateMoveAnimation(2);
         }
      }

      DoubleAnimation CreateZoomAnimation(double toValue)
      {
         var da = new DoubleAnimation(toValue, new Duration(TimeSpan.FromMilliseconds(555)));
         da.AccelerationRatio = 0.1;
         da.DecelerationRatio = 0.9;
         da.FillBehavior = FillBehavior.HoldEnd;
         da.Freeze();
         return da;
      }

      DoubleAnimation CreateMoveAnimation(double toValue)
      {
         var da = new DoubleAnimation(toValue, new Duration(TimeSpan.FromMilliseconds(555)));
         da.AccelerationRatio = 0.1;
         da.DecelerationRatio = 0.9;
         da.FillBehavior = FillBehavior.HoldEnd;
         da.Freeze();
         return da;
      }

      DoubleAnimation CreateOpacityAnimation(double toValue)
      {
         var da = new DoubleAnimation(toValue, new Duration(TimeSpan.FromMilliseconds(555)));
         da.AccelerationRatio = 0.5;
         da.DecelerationRatio = 0.5;
         da.FillBehavior = FillBehavior.HoldEnd;
         da.Freeze();
         return da;
      }

      void BeginAnimateOpacity(TileVisual target)
      {
         target.Opacity = 0.4;
         target.BeginAnimation(TileVisual.OpacityProperty, OpacityAnimation, HandoffBehavior.Compose);
      }

      void BeginAnimateZoom(TileVisual target)
      {
         //target.TranslateTransform.BeginAnimation(TranslateTransform.XProperty, MoveAnimation, HandoffBehavior.Compose);
         //target.TranslateTransform.BeginAnimation(TranslateTransform.YProperty, MoveAnimation, HandoffBehavior.Compose);
         //target.ScaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, ZoomAnimation, HandoffBehavior.Compose);
         //target.ScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, ZoomAnimation, HandoffBehavior.Compose);
      }

      DoubleAnimation OpacityAnimation;
      DoubleAnimation ZoomAnimation;
      DoubleAnimation MoveAnimation;

      QuadTree<TileVisual> QuadTree = new QuadTree<TileVisual>();

      bool update = true;
      Dictionary<RawTile, TileVisual> images = new Dictionary<RawTile, TileVisual>();
      System.Windows.Size TilesSize = new System.Windows.Size();

      Stopwatch _stopwatch = new Stopwatch();
      ushort _frameCounter;
      ushort _frameCounterUpdate;
      int count = 0;

      protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
      {
         base.OnRenderSizeChanged(sizeInfo);

         {
            System.Windows.Size constraint = sizeInfo.NewSize;
            region = new GMap.NET.Rectangle(-50, -50, (int) constraint.Width + 100, (int) constraint.Height + 100);

            TilesLayer.Width = sizeInfo.NewSize.Width;
            TilesLayer.Height = sizeInfo.NewSize.Height;

            QuadTree.Bounds = new Rect(sizeInfo.NewSize);
         }

         //var sizeInPx = Projection.GetTileMatrixSizePixel(ZoomStep);
         //TilesLayer.Width = sizeInPx.Width;
         //TilesLayer.Height = sizeInPx.Height;

         if(IsLoaded)
         {
            Refresh();
         }
      }

      void CompositionTargetEx_FrameUpdating(object sender, RenderingEventArgs e)
      {
         if(update)
         {
            _frameCounterUpdate++;
            update = false;

            #region -- add image --
            for(int x = 0; x < TilesSize.Width; x++)
            {
               for(int y = 0; y < TilesSize.Height; y++)
               {
                  var rawTile = new RawTile(MapType.GoogleHybrid, new GMap.NET.Point(x, y), ZoomStep);
                  var rectTilePx = new Rect(x*Projection.TileSize.Width, y*Projection.TileSize.Height, Projection.TileSize.Width, Projection.TileSize.Height);

                  TileVisual image = null;
                  if(!images.TryGetValue(rawTile, out image))
                  {
                     var layers = GMaps.Instance.GetAllLayersOfType(rawTile.Type);

                     ImageSource[] imgs = new ImageSource[layers.Length];

                     // get tiles
                     for(int i = 0; i < layers.Length; i++)
                     {
                        Exception ex;
                        var res = GMaps.Instance.GetImageFrom(layers[i], rawTile.Pos, rawTile.Zoom, out ex) as WindowsPresentationImage;
                        if(res != null)
                        {
                           imgs[i] = res.Img;
                        }
                     }

                     // combine visual
                     image = new TileVisual(imgs, rawTile);
                     images.Add(rawTile, image);

                     Canvas.SetZIndex(image, -1);
                  }

                  bool ri = (region.IntersectsWith(new GMap.NET.Rectangle((int) (rectTilePx.X + renderOffset.X), (int) (rectTilePx.Y + renderOffset.Y), (int) rectTilePx.Width, (int) rectTilePx.Height)));
                  if(TilesLayer.Children.Contains(image))
                  {
                     if(ri)
                     {
                        image.MoveTo(Math.Round(rectTilePx.X) + 0.6 + renderOffset.X, Math.Round(rectTilePx.Y) + 0.6 + renderOffset.Y);
                     }
                     else
                     {
                        TilesLayer.Children.Remove(image);
                     }
                  }
                  else
                  {
                     if(ri)
                     {
                        image.MoveTo(Math.Round(rectTilePx.X) + 0.6 + renderOffset.X, Math.Round(rectTilePx.Y) + 0.6 + renderOffset.Y);
                        BeginAnimateOpacity(image);
                        {
                           TilesLayer.Children.Add(image);
                        }
                     }
                  }
                  //break;
               }
               //break;
            }

            count = TilesLayer.Children.Count;

            #endregion
         }

         if(_stopwatch.ElapsedMilliseconds >= 1000)
         {
            _stopwatch.Stop();

            perfInfo.Text = "FPS: " + (ushort) (_frameCounter/_stopwatch.Elapsed.TotalSeconds) + " | " + (ushort) (_frameCounterUpdate/_stopwatch.Elapsed.TotalSeconds) + " | " + count + " tiles";

            _frameCounter = 0;
            _frameCounterUpdate = 0;
            _stopwatch.Reset();
            _stopwatch.Start();
         }
         else
         {
            _frameCounter++;
         }
      }

      void GMapControl_Loaded(object sender, RoutedEventArgs e)
      {
         CompositionTargetEx.FrameUpdating += new EventHandler<RenderingEventArgs>(CompositionTargetEx_FrameUpdating);
         _stopwatch.Start();

         Refresh();

         //Core.StartSystem();
         //Core_OnMapZoomChanged();
      }

      void GMapControl_Unloaded(object sender, RoutedEventArgs e)
      {
         CompositionTargetEx.FrameUpdating -= new EventHandler<RenderingEventArgs>(CompositionTargetEx_FrameUpdating);
         _stopwatch.Stop();

         //Core.OnMapClose();
      }

      private void Refresh()
      {
         update = true;
         InvalidateVisual();
      }

      //rotected override System.Windows.Size ArrangeOverride(System.Windows.Size finalSize)
      ///
      //MapBase parentMap = this.ParentMap;
      //foreach(UIElement element in base.Children)
      //{
      //   Rect finalRect = new Rect(0.0, 0.0, parentMap.ViewportSize.Width, parentMap.ViewportSize.Height);
      //   LocationRect positionRectangle = GetPositionRectangle(element);
      //   if(positionRectangle != null)
      //   {
      //      finalRect = parentMap.Mode.LocationToViewportPoint(positionRectangle);
      //   }
      //   else
      //   {
      //      Point point;
      //      Location position = GetPosition(element);
      //      if((position != null) && parentMap.TryLocationToViewportPoint(position, out point))
      //      {
      //         PositionOrigin positionOrigin = GetPositionOrigin(element);
      //         point.X -= positionOrigin.X * element.DesiredSize.Width;
      //         point.Y -= positionOrigin.Y * element.DesiredSize.Height;
      //         finalRect = new Rect(point.X, point.Y, element.DesiredSize.Width, element.DesiredSize.Height);
      //      }
      //   }
      //   Point positionOffset = GetPositionOffset(element);
      //   finalRect.X += positionOffset.X;
      //   finalRect.Y += positionOffset.Y;
      //   element.Arrange(finalRect);
      //}
      //return parentMap.ViewportSize;
      //

      //protected override Size MeasureOverride(Size availableSize)
      //{
      //   MapBase parentMap = this.ParentMap;
      //   Guid lastProjectPassTag = this.lastProjectPassTag;
      //   this.lastProjectPassTag = Guid.NewGuid();
      //   foreach(UIElement element in base.Children)
      //   {
      //      IProjectable projectable = element as IProjectable;
      //      if(projectable != null)
      //      {
      //         ProjectionUpdateLevel pendingUpdate = this.pendingUpdate;
      //         if(((Guid) element.GetValue(ProjectionUpdatedTag)) != lastProjectPassTag)
      //         {
      //            pendingUpdate = ProjectionUpdateLevel.Full;
      //         }
      //         if(pendingUpdate != ProjectionUpdateLevel.None)
      //         {
      //            projectable.ProjectionUpdated(pendingUpdate);
      //         }
      //         element.SetValue(ProjectionUpdatedTag, this.lastProjectPassTag);
      //      }
      //   }
      //   this.pendingUpdate = ProjectionUpdateLevel.None;
      //   foreach(UIElement element2 in base.Children)
      //   {
      //      LocationRect positionRectangle = GetPositionRectangle(element2);
      //      if(positionRectangle != null)
      //      {
      //         Rect rect2 = parentMap.Mode.LocationToViewportPoint(positionRectangle);
      //         element2.Measure(new Size(rect2.Width, rect2.Height));
      //      }
      //      else
      //      {
      //         if((element2 is ContentPresenter) && (VisualTreeHelper.GetChildrenCount(element2) > 0))
      //         {
      //            IProjectable child = VisualTreeHelper.GetChild(element2, 0) as IProjectable;
      //            if(child != null)
      //            {
      //               child.ProjectionUpdated(ProjectionUpdateLevel.Full);
      //               UIElement element3 = child as UIElement;
      //               if(element3 != null)
      //               {
      //                  element3.InvalidateMeasure();
      //               }
      //            }
      //         }
      //         element2.Measure(parentMap.ViewportSize);
      //      }
      //   }
      //   return parentMap.ViewportSize;
      //}

      #region -- etc --
      void Core_OnMapZoomChanged()
      {
         //UpdateMarkersOffset();

         foreach(var i in Markers)
         {
            //i.ForceUpdateLocalPosition(this);
         }

         var routes = Markers.Where(p => p != null && p.Route.Count > 1);
         if(routes != null)
         {
            foreach(var i in routes)
            {
               //i.RegenerateRouteShape(this);
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
            this.Dispatcher.BeginInvoke(DispatcherPriority.Render, new MethodInvoker(Refresh));
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
      #endregion

      #region UserControl Events

      //double move;
      protected override void OnMouseWheel(MouseWheelEventArgs e)
      {
         base.OnMouseWheel(e);

         if(e.Delta > 0)
         {
            ZoomStep++;

            //ZoomAnimation = CreateZoomAnimation(ZoomStep);

            // move -= Projection.TileSize.Width;
            //MoveAnimation = CreateMoveAnimation(move); 
         }
         else
         {
            ZoomStep--;

            //ZoomAnimation = CreateZoomAnimation(ZoomStep);

            //move += Projection.TileSize.Width;
            //MoveAnimation = CreateMoveAnimation(move);
         }
         //Refresh();
      }

      bool isSelected = false;

      System.Windows.Point? mouseDown = null;
      static readonly System.Windows.Point Empty = new System.Windows.Point();

      protected override void OnMouseDown(MouseButtonEventArgs e)
      {
         if(CanDragMap && e.ChangedButton == DragButton && e.ButtonState == MouseButtonState.Pressed)
         {
            mouseDown = e.GetPosition(TilesLayer);

            dragPoint.X = mouseDown.Value.X - renderOffset.X;
            dragPoint.Y = mouseDown.Value.Y - renderOffset.Y;

            Mouse.Capture(TilesLayer);
         }

         base.OnMouseDown(e);
      }

      protected override void OnMouseUp(MouseButtonEventArgs e)
      {
         base.OnMouseUp(e);

         mouseDown = null;
         Mouse.Capture(null);
      }

      System.Windows.Point renderOffset = new System.Windows.Point();
      System.Windows.Point dragPoint = new System.Windows.Point();

      protected override void OnMouseMove(MouseEventArgs e)
      {
         base.OnMouseMove(e);

         if(mouseDown.HasValue)
         {
            System.Windows.Point p = e.GetPosition(TilesLayer);

            //TileOffset.Y += p.Y - mouseDown.Value.Y;
            //TileOffset.X += p.X - mouseDown.Value.X;            

            renderOffset.X = p.X - dragPoint.X;
            renderOffset.Y = p.Y - dragPoint.Y;

            Refresh();
         }
      }

      #endregion

      #region IGControl Members

      public void ReloadMap()
      {
         Core.ReloadMap();
      }

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

      public PointLatLng FromLocalToLatLng(int x, int y)
      {
         if(MapRenderTransform != null)
         {
            var tp = MapRenderTransform.Inverse.Transform(new System.Windows.Point(x, y));
            x = (int) tp.X;
            y = (int) tp.Y;
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
#if SQLite
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
#endif
         return false;
      }

      public bool ShowImportDialog()
      {
#if SQLite
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
#endif
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
               bool changed = (Core.Zoom != value);
               Core.Zoom = value;
               if(changed)
               {
                  foreach(UIElement u in TilesLayer.Children)
                  {
                     var m = u as TileVisual;
                     if(m != null)
                     {
                        if(images.ContainsKey(m.Tile))
                        {
                           images.Remove(m.Tile);
                           m.Source.Clear();
                        }
                     }
                  }

                  TilesLayer.Children.Clear();

                  var sizeinTiles = Projection.GetTileMatrixSizeXY(ZoomStep);
                  TilesSize.Width = sizeinTiles.Width;
                  TilesSize.Height = sizeinTiles.Height;

                  if(IsLoaded)
                  {
                     Refresh();
                  }
               }
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

   static class Extensions
   {
      public static void CenterAt(this FrameworkElement element, System.Windows.Point center)
      {
         CenterAt(element, center.X, center.Y);
      }

      public static void CenterAt(this FrameworkElement element, double x, double y)
      {
         MoveTo(element, x - element.Width / 2, y - element.Height / 2);
      }

      public static void MoveTo(this UIElement element, double x, double y)
      {
         Canvas.SetLeft(element, x);
         Canvas.SetTop(element, y);
      }

      public static void MoveTo(this UIElement element, System.Windows.Point position)
      {
         MoveTo(element, position.X, position.Y);
      }

      public static void MoveOffset(this UIElement element, double xOffset, double yOffset)
      {
         if(element == null || double.IsNaN(xOffset) || double.IsNaN(yOffset))
         {
            return;
         }
         var coordinates = element.GetCoordinates();
         Canvas.SetLeft(element, coordinates.X + xOffset);
         Canvas.SetTop(element, coordinates.Y + yOffset);
      }

      public static System.Windows.Point GetCoordinates(this UIElement element)
      {
         return new System.Windows.Point(Canvas.GetLeft(element), Canvas.GetTop(element));
      }

      public static System.Windows.Rect GetRect(this UIElement element)
      {
         return new System.Windows.Rect(element.GetCoordinates(), element.RenderSize);
      }
   }
}