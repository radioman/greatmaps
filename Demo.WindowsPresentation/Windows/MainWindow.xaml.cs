using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Demo.WindowsPresentation.CustomMarkers;
using GMap.NET;
using GMap.NET.WindowsPresentation;

namespace Demo.WindowsPresentation
{
   public partial class MainWindow : Window
   {
      PointLatLng start;
      PointLatLng end;

      // marker
      GMapMarker currentMarker;

      // zones list
      List<GMapMarker> Circles = new List<GMapMarker>();

      public MainWindow()
      {
         InitializeComponent();

         // add your custom map db provider
         //MySQLPureImageCache ch = new MySQLPureImageCache();
         //ch.ConnectionString = @"server=sql2008;User Id=trolis;Persist Security Info=True;database=gmapnetcache;password=trolis;";
         //MainMap.Manager.ImageCacheSecond = ch;

         // set your proxy here if need
         //MainMap.Manager.Proxy = new WebProxy("10.2.0.100", 8080);
         //MainMap.Manager.Proxy.Credentials = new NetworkCredential("ogrenci@bilgeadam.com", "bilgeadam");

         // set cache mode only if no internet avaible
         try
         {
            System.Net.IPHostEntry e = System.Net.Dns.GetHostEntry("www.google.com");
         }
         catch
         {
            MainMap.Manager.Mode = AccessMode.CacheOnly;
            MessageBox.Show("No internet connection avaible, going to CacheOnly mode.", "GMap.NET - Demo.WindowsPresentation", MessageBoxButton.OK, MessageBoxImage.Warning);
         }

         // config map
         MainMap.MapType = MapType.GoogleMap;
         MainMap.MaxZoom = 17;
         MainMap.MinZoom = 5;
         MainMap.Zoom = 13;
         MainMap.CurrentPosition = new PointLatLng(54.6961334816182, 25.2985095977783);

         // map events
         MainMap.OnCurrentPositionChanged += new CurrentPositionChanged(MainMap_OnCurrentPositionChanged);
         MainMap.OnTileLoadComplete += new TileLoadComplete(MainMap_OnTileLoadComplete);
         MainMap.OnTileLoadStart += new TileLoadStart(MainMap_OnTileLoadStart);
         MainMap.OnMapZoomChanged += new MapZoomChanged(MainMap_OnMapZoomChanged);
         MainMap.OnMapTypeChanged += new MapTypeChanged(MainMap_OnMapTypeChanged);
         MainMap.MouseMove += new System.Windows.Input.MouseEventHandler(MainMap_MouseMove);
         MainMap.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(MainMap_MouseLeftButtonDown);

         // get map types
         comboBoxMapType.ItemsSource = Enum.GetValues(typeof(MapType));
         comboBoxMapType.SelectedItem = MainMap.MapType;

         // acccess mode
         comboBoxMode.ItemsSource = Enum.GetValues(typeof(AccessMode));
         comboBoxMode.SelectedItem = MainMap.Manager.Mode;

         // get cache modes
         checkBoxCacheRoute.IsChecked = MainMap.Manager.UseRouteCache;
         checkBoxGeoCache.IsChecked = MainMap.Manager.UseGeocoderCache;

         // setup zoom slider
         sliderZoom.Maximum = MainMap.MaxZoom;
         sliderZoom.Minimum = MainMap.MinZoom;
         sliderZoom.Value = MainMap.Zoom;

         // get position
         textBoxLat.Text = MainMap.CurrentPosition.Lat.ToString(CultureInfo.InvariantCulture);
         textBoxLng.Text = MainMap.CurrentPosition.Lng.ToString(CultureInfo.InvariantCulture);

         // get marker state
         checkBoxCurrentMarker.IsChecked = true;

         // can drag map
         checkBoxDragMap.IsChecked = MainMap.CanDragMap;

#if DEBUG
         checkBoxDebug.IsChecked = true;
#endif

         // set current marker
         currentMarker = new GMapMarker(MainMap.CurrentPosition);
         {
            currentMarker.Shape = new CustomMarkerRed(this, currentMarker, "custom position marker");
            currentMarker.Offset = new System.Windows.Point(-15, -15);
            currentMarker.ZIndex = int.MaxValue;
            MainMap.Markers.Add(currentMarker);
         }

         //if(false)
         {
            // add my city location for demo
            GeoCoderStatusCode status = GeoCoderStatusCode.Unknow;

            PointLatLng? city = GMaps.Instance.GetLatLngFromGeocoder("Lithuania, Vilnius", out status);
            if(city != null && status == GeoCoderStatusCode.G_GEO_SUCCESS)
            {
               GMapMarker it = new GMapMarker(city.Value);
               {
                  it.ZIndex = 55;
                  it.Shape = new CustomMarkerDemo(this, it, "Welcome to Lithuania! ;}");
               }
               MainMap.Markers.Add(it);

               #region -- add some markers and zone around them --
               {
                  List<PointAndInfo> objects = new List<PointAndInfo>();
                  {
                     string area = "Antakalnis";
                     PointLatLng? pos = GMaps.Instance.GetLatLngFromGeocoder("Lithuania, Vilnius, " + area, out status);
                     if(pos != null && status == GeoCoderStatusCode.G_GEO_SUCCESS)
                     {
                        objects.Add(new PointAndInfo(pos.Value, area));
                     }
                  }
                  {
                     string area = "Senamiestis";
                     PointLatLng? pos = GMaps.Instance.GetLatLngFromGeocoder("Lithuania, Vilnius, " + area, out status);
                     if(pos != null && status == GeoCoderStatusCode.G_GEO_SUCCESS)
                     {
                        objects.Add(new PointAndInfo(pos.Value, area));
                     }
                  }
                  {
                     string area = "Pilaite";
                     PointLatLng? pos = GMaps.Instance.GetLatLngFromGeocoder("Lithuania, Vilnius, " + area, out status);
                     if(pos != null && status == GeoCoderStatusCode.G_GEO_SUCCESS)
                     {
                        objects.Add(new PointAndInfo(pos.Value, area));
                     }
                  }
                  AddDemoZone(8.8, city.Value, objects);
               }
               #endregion
            }
         }

         // perfromance test
         timer.Interval = TimeSpan.FromMilliseconds(44);
         timer.Tick += new EventHandler(timer_Tick);

         // transport demo
         transport.DoWork += new DoWorkEventHandler(transport_DoWork);
         transport.ProgressChanged += new ProgressChangedEventHandler(transport_ProgressChanged);
         transport.WorkerSupportsCancellation = true;
         transport.WorkerReportsProgress = true;
      }

      #region -- performance test--
      public RenderTargetBitmap ToImageSource(FrameworkElement obj)
      {
         // Save current canvas transform
         Transform transform = obj.LayoutTransform;
         obj.LayoutTransform = null;

         // fix margin offset as well
         Thickness margin = obj.Margin;
         obj.Margin = new Thickness(0, 0, margin.Right - margin.Left, margin.Bottom - margin.Top);

         // Get the size of canvas
         System.Windows.Size size = new System.Windows.Size(obj.Width, obj.Height);

         // force control to Update
         obj.Measure(size);
         obj.Arrange(new Rect(size));

         RenderTargetBitmap bmp = new RenderTargetBitmap((int) size.Width, (int) size.Height, 96, 96, PixelFormats.Pbgra32);
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

      double NextDouble(Random rng, double min, double max)
      {
         return min + (rng.NextDouble() * (max - min));
      }

      Random r = new Random();

      int tt = 0;
      void timer_Tick(object sender, EventArgs e)
      {
         var pos = new PointLatLng(NextDouble(r, MainMap.CurrentViewArea.Top, MainMap.CurrentViewArea.Bottom), NextDouble(r, MainMap.CurrentViewArea.Left, MainMap.CurrentViewArea.Right));
         GMapMarker m = new GMapMarker(pos);
         {
            var s = new Test((tt++).ToString());

            var image = new Image();
            {
               RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.LowQuality);
               image.Stretch = Stretch.None;
               image.Opacity = s.Opacity;

               image.MouseEnter += new System.Windows.Input.MouseEventHandler(image_MouseEnter);
               image.MouseLeave += new System.Windows.Input.MouseEventHandler(image_MouseLeave);

               image.Source = ToImageSource(s);
            }

            m.Shape = image;

            m.Offset = new System.Windows.Point(-s.Width, -s.Height);
         }
         MainMap.Markers.Add(m);

         if(tt >= 333)
         {
            timer.Stop();
            tt = 0;
         }
      }

      void image_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
      {
         Image img = sender as Image;
         img.RenderTransform = null;
      }

      void image_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
      {
         Image img = sender as Image;
         img.RenderTransform = new ScaleTransform(1.2, 1.2, 12.5, 12.5);
      }

      DispatcherTimer timer = new DispatcherTimer();
      #endregion

      #region -- transport demo --
      BackgroundWorker transport = new BackgroundWorker();

      readonly List<VehicleData> trolleybus = new List<VehicleData>();
      readonly Dictionary<int, GMapMarker> trolleybusMarkers = new Dictionary<int, GMapMarker>();

      readonly List<VehicleData> bus = new List<VehicleData>();
      readonly Dictionary<int, GMapMarker> busMarkers = new Dictionary<int, GMapMarker>();

      bool firstLoadTrasport = true;

      void transport_ProgressChanged(object sender, ProgressChangedEventArgs e)
      {
         lock(trolleybus)
         {
            foreach(VehicleData d in trolleybus)
            {
               GMapMarker marker;

               if(!trolleybusMarkers.TryGetValue(d.Id, out marker))
               {
                  marker = new GMapMarker(new PointLatLng(d.Lat, d.Lng));
                  marker.Tag = d.Id;
                  marker.Shape = new CircleVisual(marker, Brushes.Red);

                  trolleybusMarkers[d.Id] = marker;
                  MainMap.Markers.Add(marker);
               }
               else
               {
                  marker.Position = new PointLatLng(d.Lat, d.Lng);
                  var shape = (marker.Shape as CircleVisual);
                  {
                     shape.Text = d.Line;
                     shape.Angle = d.Bearing;
                     shape.Tooltip.SetValues("TrolleyBus", d);

                     if(shape.IsChanged)
                     {
                        shape.UpdateVisual(false);
                     }
                  }
               }
            }
         }

         lock(bus)
         {
            foreach(VehicleData d in bus)
            {
               GMapMarker marker;

               if(!busMarkers.TryGetValue(d.Id, out marker))
               {
                  marker = new GMapMarker(new PointLatLng(d.Lat, d.Lng));
                  marker.Tag = d.Id;
                  marker.Shape = new CircleVisual(marker, Brushes.Blue);

                  busMarkers[d.Id] = marker;
                  MainMap.Markers.Add(marker);
               }
               else
               {
                  marker.Position = new PointLatLng(d.Lat, d.Lng);
                  var shape = (marker.Shape as CircleVisual);
                  {
                     shape.Text = d.Line;
                     shape.Angle = d.Bearing;
                     shape.Tooltip.SetValues("Bus", d);

                     if(shape.IsChanged)
                     {
                        shape.UpdateVisual(false);
                     }
                  }
               }
            }
         }

         if(firstLoadTrasport)
         {
            firstLoadTrasport = false;
         }
      }

      void transport_DoWork(object sender, DoWorkEventArgs e)
      {
         while(!transport.CancellationPending)
         {
            try
            {
               lock(trolleybus)
               {
                  MainMap.Manager.GetVilniusTransportData(TransportType.TrolleyBus, string.Empty, trolleybus);
               }

               lock(bus)
               {
                  MainMap.Manager.GetVilniusTransportData(TransportType.Bus, string.Empty, bus);
               }

               transport.ReportProgress(100);
            }
            catch(Exception ex)
            {
               Debug.WriteLine("transport_DoWork: " + ex.ToString());
            }
            Thread.Sleep(3333);
         }
         trolleybusMarkers.Clear();
         busMarkers.Clear();
      }

      #endregion

      // add objects and zone around them
      void AddDemoZone(double areaRadius, PointLatLng center, List<PointAndInfo> objects)
      {
         var objectsInArea = from p in objects
                             where GMaps.Instance.GetDistance(center, p.Point) <= areaRadius
                             select new
                             {
                                Obj = p,
                                Dist = GMaps.Instance.GetDistance(center, p.Point)
                             };
         if(objectsInArea.Any())
         {
            var maxDistObject = (from p in objectsInArea
                                 orderby p.Dist descending
                                 select p).First();

            // add objects to zone
            foreach(var o in objectsInArea)
            {
               GMapMarker it = new GMapMarker(o.Obj.Point);
               {
                  it.ZIndex = 55;
                  var s = new CustomMarkerDemo(this, it, o.Obj.Info + ", distance from center: " + o.Dist + "km.");
                  it.Shape = s;
               }
               MainMap.Markers.Add(it);
            }

            // add zone circle
            {
               GMapMarker it = new GMapMarker(center);
               it.ZIndex = -1;

               Circle c = new Circle();
               c.Center = center;
               c.Bound = maxDistObject.Obj.Point;
               c.Tag = it;
               c.IsHitTestVisible = false;

               UpdateCircle(c);
               Circles.Add(it);

               it.Shape = c;
               MainMap.Markers.Add(it);
            }
         }
      }

      // calculates circle radius
      void UpdateCircle(Circle c)
      {
         var pxCenter = MainMap.FromLatLngToLocal(c.Center);
         var pxBounds = MainMap.FromLatLngToLocal(c.Bound);

         double a = (double) (pxBounds.X - pxCenter.X);
         double b = (double) (pxBounds.Y - pxCenter.Y);
         var pxCircleRadius = Math.Sqrt(a * a + b * b);

         c.Width = 55 + pxCircleRadius * 2;
         c.Height = 55 + pxCircleRadius * 2;
         (c.Tag as GMapMarker).Offset = new System.Windows.Point(-c.Width / 2, -c.Height / 2);
      }

      void MainMap_OnMapTypeChanged(MapType type)
      {
         sliderZoom.Minimum = MainMap.MinZoom;
         sliderZoom.Maximum = MainMap.MaxZoom;
      }

      void MainMap_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
      {
         System.Windows.Point p = e.GetPosition(MainMap);
         currentMarker.Position = MainMap.FromLocalToLatLng((int) p.X, (int) p.Y);
      }

      // move current marker with left holding
      void MainMap_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
      {
         if(e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
         {
            System.Windows.Point p = e.GetPosition(MainMap);
            currentMarker.Position = MainMap.FromLocalToLatLng((int) p.X, (int) p.Y);
         }
      }

      private void button13_Click(object sender, RoutedEventArgs e)
      {
         MainMap.ZoomAndCenterMarkers(55);
      }

      // MapZoomChanged
      void MainMap_OnMapZoomChanged()
      {
         sliderZoom.Value = MainMap.Zoom;
      }

      // tile louading starts
      void MainMap_OnTileLoadStart()
      {
         System.Windows.Forms.MethodInvoker m = delegate()
         {
            progressBar1.Visibility = Visibility.Visible;
         };

         try
         {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, m);
         }
         catch
         {
         }
      }

      // tile loading stops
      void MainMap_OnTileLoadComplete()
      {
         System.Windows.Forms.MethodInvoker m = delegate()
         {
            progressBar1.Visibility = Visibility.Hidden;
         };

         try
         {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, m);
         }
         catch
         {
         }
      }

      // current location changed
      void MainMap_OnCurrentPositionChanged(PointLatLng point)
      {
         mapgroup.Header = "gmap: " + point;
      }

      // reload
      private void button1_Click(object sender, RoutedEventArgs e)
      {
         MainMap.ReloadMap();
      }

      // map type changed
      private void comboBoxMapType_DropDownClosed(object sender, EventArgs e)
      {
         MainMap.MapType = (MapType) comboBoxMapType.SelectedItem;
      }

      // enable current marker
      private void checkBoxCurrentMarker_Checked(object sender, RoutedEventArgs e)
      {
         if(currentMarker != null)
         {
            MainMap.Markers.Add(currentMarker);
         }
      }

      // disable current marker
      private void checkBoxCurrentMarker_Unchecked(object sender, RoutedEventArgs e)
      {
         if(currentMarker != null)
         {
            MainMap.Markers.Remove(currentMarker);
         }
      }

      // enable map dragging
      private void checkBoxDragMap_Checked(object sender, RoutedEventArgs e)
      {
         MainMap.CanDragMap = true;
      }

      // disable map dragging
      private void checkBoxDragMap_Unchecked(object sender, RoutedEventArgs e)
      {
         MainMap.CanDragMap = false;
      }

      // goto!
      private void button2_Click(object sender, RoutedEventArgs e)
      {
         double lat = double.Parse(textBoxLat.Text, CultureInfo.InvariantCulture);
         double lng = double.Parse(textBoxLng.Text, CultureInfo.InvariantCulture);

         currentMarker.Position = new PointLatLng(lat, lng);

         MainMap.CurrentPosition = currentMarker.Position;
      }

      // goto by geocoder
      private void textBoxGeo_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
      {
         if(e.Key == System.Windows.Input.Key.Enter)
         {
            GeoCoderStatusCode status = MainMap.SetCurrentPositionByKeywords(textBoxGeo.Text);
            if(status != GeoCoderStatusCode.G_GEO_SUCCESS)
            {
               MessageBox.Show("Google Maps Geocoder can't find: '" + textBoxGeo.Text + "', reason: " + status.ToString(), "GMap.NET", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
         }
      }

      // zoom changed
      private void sliderZoom_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
      {
         MainMap.Zoom = e.NewValue;

         // updates circles on map
         foreach(var c in Circles)
         {
            UpdateCircle(c.Shape as Circle);
         }
      }

      // zoom up
      private void czuZoomUp_Click(object sender, RoutedEventArgs e)
      {
         sliderZoom.Value = ((int) MainMap.Zoom) + 1;
      }

      // zoom down
      private void czuZoomDown_Click(object sender, RoutedEventArgs e)
      {
         sliderZoom.Value = ((int) (MainMap.Zoom + 0.99)) - 1;
      }

      // prefetch
      private void button3_Click(object sender, RoutedEventArgs e)
      {
         RectLatLng area = MainMap.SelectedArea;
         if(!area.IsEmpty)
         {
            for(int i = (int) MainMap.Zoom; i <= MainMap.MaxZoom; i++)
            {
               var x = MainMap.Projection.GetAreaTileList(area, i, 0);

               MessageBoxResult res = MessageBox.Show("Ready ripp at Zoom = " + i + " ? Total => " + x.Count, "GMap.NET", MessageBoxButton.YesNoCancel);

               if(res == MessageBoxResult.Yes)
               {
                  TilePrefetcher obj = new TilePrefetcher();
                  obj.ShowCompleteMessage = true;
                  obj.Start(x, i, MainMap.MapType, 100);
               }
               else
                  if(res == MessageBoxResult.No)
                  {
                     continue;
                  }
                  else
                     if(res == MessageBoxResult.Cancel)
                     {
                        break;
                     }

               x.Clear();
            }
         }
         else
         {
            MessageBox.Show("Select map area holding ALT", "GMap.NET", MessageBoxButton.OK, MessageBoxImage.Exclamation);
         }
      }

      // access mode
      private void comboBoxMode_DropDownClosed(object sender, EventArgs e)
      {
         MainMap.Manager.Mode = (AccessMode) comboBoxMode.SelectedItem;
         MainMap.ReloadMap();
      }

      // clear cache
      private void button4_Click(object sender, RoutedEventArgs e)
      {
         if(MessageBox.Show("Are You sure?", "Clear GMap.NET cache?", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
         {
            try
            {
               System.IO.Directory.Delete(MainMap.CacheLocation, true);
            }
            catch(Exception ex)
            {
               MessageBox.Show(ex.Message);
            }
         }
      }

      // export
      private void button6_Click(object sender, RoutedEventArgs e)
      {
         MainMap.ShowExportDialog();
      }

      // import
      private void button5_Click(object sender, RoutedEventArgs e)
      {
         MainMap.ShowImportDialog();
      }

      // use route cache
      private void checkBoxCacheRoute_Checked(object sender, RoutedEventArgs e)
      {
         GMaps.Instance.UseRouteCache = checkBoxCacheRoute.IsChecked.Value;
      }

      // use geocoding cahce
      private void checkBoxGeoCache_Checked(object sender, RoutedEventArgs e)
      {
         GMaps.Instance.UseGeocoderCache = checkBoxGeoCache.IsChecked.Value;
         GMaps.Instance.UsePlacemarkCache = GMaps.Instance.UseGeocoderCache;
      }

      // save currnt view
      private void button7_Click(object sender, RoutedEventArgs e)
      {
         try
         {
            ImageSource img = MainMap.ToImageSource();
            PngBitmapEncoder en = new PngBitmapEncoder();
            en.Frames.Add(BitmapFrame.Create(img as BitmapSource));

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "GMap.NET Image"; // Default file name
            dlg.DefaultExt = ".png"; // Default file extension
            dlg.Filter = "Image (.png)|*.png"; // Filter files by extension
            dlg.AddExtension = true;
            dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

            // Show save file dialog box
            bool? result = dlg.ShowDialog();

            // Process save file dialog box results
            if(result == true)
            {
               // Save document
               string filename = dlg.FileName;

               using(System.IO.Stream st = System.IO.File.OpenWrite(filename))
               {
                  en.Save(st);
               }
            }
         }
         catch(Exception ex)
         {
            MessageBox.Show(ex.Message);
         }
      }

      // clear all markers
      private void button10_Click(object sender, RoutedEventArgs e)
      {
         var clear = MainMap.Markers.Where(p => p != null && p != currentMarker);
         if(clear != null)
         {
            for(int i = 0; i < clear.Count(); i++)
            {
               MainMap.Markers.Remove(clear.ElementAt(i));
               i--;
            }
         }

         if(radioButtonPerformance.IsChecked == true)
         {
            tt = 0;
            if(!timer.IsEnabled)
            {
               timer.Start();
            }
         }
      }

      // add marker
      private void button8_Click(object sender, RoutedEventArgs e)
      {
         GMapMarker m = new GMapMarker(currentMarker.Position);
         {
            Placemark p = null;
            if(checkBoxPlace.IsChecked.Value)
            {
               p = GMaps.Instance.GetPlacemarkFromGeocoder(currentMarker.Position);
            }

            string ToolTipText;
            if(p != null)
            {
               ToolTipText = p.Address;
            }
            else
            {
               ToolTipText = currentMarker.Position.ToString();
            }

            m.Shape = new CustomMarkerDemo(this, m, ToolTipText);
            m.ZIndex = 55;
         }
         MainMap.Markers.Add(m);
      }

      // sets route start
      private void button11_Click(object sender, RoutedEventArgs e)
      {
         start = currentMarker.Position;
      }

      // sets route end
      private void button9_Click(object sender, RoutedEventArgs e)
      {
         end = currentMarker.Position;
      }

      // adds route
      private void button12_Click(object sender, RoutedEventArgs e)
      {
         MapRoute route = GMaps.Instance.GetRouteBetweenPoints(start, end, false, (int) MainMap.Zoom);
         if(route != null)
         {
            GMapMarker m1 = new GMapMarker(start);
            m1.Shape = new CustomMarkerDemo(this, m1, "Start: " + route.Name);

            GMapMarker m2 = new GMapMarker(end);
            m2.Shape = new CustomMarkerDemo(this, m2, "End: " + start.ToString());

            GMapMarker mRoute = new GMapMarker(start);
            {
               mRoute.Route.AddRange(route.Points);
               mRoute.RegenerateRouteShape(MainMap);

               mRoute.ZIndex = -1;
            }

            MainMap.Markers.Add(m1);
            MainMap.Markers.Add(m2);
            MainMap.Markers.Add(mRoute);
         }
      }

      // enables tile grid view
      private void checkBox1_Checked(object sender, RoutedEventArgs e)
      {
         MainMap.ShowTileGridLines = true;
      }

      // disables tile grid view
      private void checkBox1_Unchecked(object sender, RoutedEventArgs e)
      {
         MainMap.ShowTileGridLines = false;
      }

      private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
      {
         int offset = 22;

         if(e.Key == Key.Left)
         {
            MainMap.Offset(-offset, 0);
         }
         else if(e.Key == Key.Right)
         {
            MainMap.Offset(offset, 0);
         }
         else if(e.Key == Key.Up)
         {
            MainMap.Offset(0, -offset);
         }
         else if(e.Key == Key.Down)
         {
            MainMap.Offset(0, offset);
         }
      }

      private void realTimeChanged(object sender, RoutedEventArgs e)
      {
         MainMap.Markers.Clear();

         // start performance test
         if(radioButtonPerformance.IsChecked == true)
         {
            timer.Start();
         }
         else
         {
            // stop performance test
            timer.Stop();
         }

         // start realtime transport tracking demo
         if(radioButtonTransport.IsChecked == true)
         {
            if(!transport.IsBusy)
            {
               firstLoadTrasport = true;
               transport.RunWorkerAsync();
            }
         }
         else
         {
            if(transport.IsBusy)
            {
               transport.CancelAsync();
            }
         }
      }
   }
}
