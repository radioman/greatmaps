using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using GMapNET;
using System.Windows.Threading;

namespace Demo.WindowsPresentation
{
   public partial class MainWindow : Window
   {
      PointLatLng start;
      PointLatLng end;

      // marker
      GMapMarker currentMarker;

      public MainWindow()
      {
         InitializeComponent();

         // config form and add map
         this.Background = Brushes.AliceBlue;

         // config gmaps
         GMaps.Instance.Language = "lt";
         GMaps.Instance.UseRouteCache = true;
         GMaps.Instance.UseGeocoderCache = true;
         GMaps.Instance.UsePlacemarkCache = true;
         GMaps.Instance.Mode = AccessMode.ServerAndCache;

         // add your custom map db provider
         //MSSQLPureImageCache ch = new MSSQLPureImageCache();
         //ch.ConnectionString = @"Data Source=SQL2008\SQLSRV08;Initial Catalog=PFleet;Integrated Security=True";
         //GMaps.Instance.ImageCacheSecond = ch;

         // set your proxy here if need
         //GMaps.Instance.Proxy = new WebProxy("10.2.0.100", 8080);
         //GMaps.Instance.Proxy.Credentials = new NetworkCredential("ogrenci@bilgeadam.com", "bilgeadam");

         // config map
         MainMap.MapType = MapType.OpenStreetMap;
         MainMap.MaxZoom = 17;
         MainMap.MinZoom = 12;
         MainMap.Zoom = MainMap.MinZoom;
         MainMap.CurrentPosition = new PointLatLng(54.6961334816182, 25.2985095977783);

         // map events
         MainMap.OnCurrentPositionChanged += new CurrentPositionChanged(MainMap_OnCurrentPositionChanged);
         MainMap.OnTileLoadComplete += new TileLoadComplete(MainMap_OnTileLoadComplete);
         MainMap.OnTileLoadStart += new TileLoadStart(MainMap_OnTileLoadStart);
         MainMap.OnEmptyTileError += new EmptyTileError(MainMap_OnEmptyTileError);
         MainMap.OnMapZoomChanged += new MapZoomChanged(MainMap_OnMapZoomChanged);

         // get map types
         comboBoxMapType.ItemsSource = Enum.GetValues(typeof(MapType));
         comboBoxMapType.SelectedItem = MainMap.MapType;

         // acccess mode
         comboBoxMode.ItemsSource = Enum.GetValues(typeof(AccessMode));
         comboBoxMode.SelectedItem = GMaps.Instance.Mode;

         // get cache modes
         checkBoxCacheRoute.IsChecked = GMaps.Instance.UseRouteCache;
         checkBoxGeoCache.IsChecked = GMaps.Instance.UseGeocoderCache;

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

         // set current marker
         currentMarker = new GMapMarker(MainMap, MainMap.CurrentPosition);
         {
            Shape el = new Cross();
            {
               el.Width = 25;
               el.Height = 25;
               el.Stroke = Brushes.Blue;
               el.Fill = Brushes.Yellow;
               el.StrokeThickness = 2;
            }
            currentMarker.Shape = el;
            currentMarker.ZIndex = int.MaxValue;
            MainMap.Markers.Add(currentMarker);
         }

         // add my city location for demo
         PointLatLng? pos = GMaps.Instance.GetLatLngFromGeocoder("Lithuania, Vilnius");
         if(pos != null)
         {
            GMapMarker it = new GMapMarker(MainMap, pos.Value);
            {
               it.Shape = new CustomMarkerDemo(it, "Welcome to Lithuania! ;}");
            }
            MainMap.Markers.Add(it);
         }
      }

      // empty tile displayed
      void MainMap_OnEmptyTileError(int zoom, GMapNET.Point pos)
      {
         // we get that exception:
         // Dispatcher processing has been suspended,
         // but messages are still being processed.

         // any ideas? ;}

         // MessageBox.Show("OnEmptyTileError, Zoom: " + zoom + ", " + pos.ToString(), "GMap.NET", MessageBoxButton.OK, MessageBoxImage.Warning);
      }

      // MapZoomChanged
      void MainMap_OnMapZoomChanged()
      {
         sliderZoom.Value = MainMap.Zoom;
      }

      // tile louading starts
      void MainMap_OnTileLoadStart(int loaderId)
      {
         switch(loaderId)
         {
            case 1:
            progressBar1.Visibility = Visibility.Visible;
            break;

            case 2:
            progressBar2.Visibility = Visibility.Visible;
            break;

            case 3:
            progressBar3.Visibility = Visibility.Visible;
            break;
         }
      }

      // tile loading stops
      void MainMap_OnTileLoadComplete(int loaderId)
      {
         switch(loaderId)
         {
            case 1:
            progressBar1.Visibility = Visibility.Hidden;
            break;

            case 2:
            progressBar2.Visibility = Visibility.Hidden;
            break;

            case 3:
            progressBar3.Visibility = Visibility.Hidden;
            break;
         }
      }

      // current location changed
      void MainMap_OnCurrentPositionChanged(PointLatLng point)
      {
         textBoxCurrLat.Text = point.Lat.ToString(CultureInfo.InvariantCulture);
         textBoxCurrLng.Text = point.Lng.ToString(CultureInfo.InvariantCulture);

         currentMarker.Position = point;
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
         MainMap.ReloadMap();
      }

      // enable current marker
      private void checkBoxCurrentMarker_Checked(object sender, RoutedEventArgs e)
      {
         if(!MainMap.Markers.Contains(currentMarker))
         {
            MainMap.Markers.Add(currentMarker);
         }
      }

      // disable current marker
      private void checkBoxCurrentMarker_Unchecked(object sender, RoutedEventArgs e)
      {
         if(MainMap.Markers.Contains(currentMarker))
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

         MainMap.CurrentPosition = new PointLatLng(lat, lng);
         MainMap.GoToCurrentPosition();
      }

      // goto by geocoder
      private void textBoxGeo_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
      {
         if(e.Key == System.Windows.Input.Key.Enter)
         {
            if(!MainMap.SetCurrentPositionByKeywords(textBoxGeo.Text))
            {
               MessageBox.Show("Google Maps Geocoder can't find: " + textBoxGeo.Text, "GMap.NET", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            else
            {
               MainMap.GoToCurrentPosition();
            }
         }
      }

      // zoom changed
      private void sliderZoom_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
      {
         int zn = (int) e.NewValue;
         {
            MainMap.Zoom = zn;
         }
      }

      private void czuZoomUp_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
      {
         MainMap.Zoom++;
         sliderZoom.Value = MainMap.Zoom;
      }

      private void czuZoomDown_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
      {
         MainMap.Zoom--;
         sliderZoom.Value = MainMap.Zoom;
      }

      // prefetch
      private void button3_Click(object sender, RoutedEventArgs e)
      {
         RectLatLng area = MainMap.CurrentViewArea;

         for(int i = MainMap.Zoom; i <= GMaps.Instance.MaxZoom; i++)
         {
            var x = GMaps.Instance.GetAreaTileList(area, i);

            MessageBoxResult res = MessageBox.Show("Ready ripp at Zoom = " + i + " ? Total => " + x.Count, "GMap.NET", MessageBoxButton.YesNoCancel);

            if(res == MessageBoxResult.Yes)
            {
               TilePrefetcher obj = new TilePrefetcher();
               obj.ShowCompleteMessage = true;
               obj.Start(x, i, MainMap.MapType, 100);
            }
            else if(res == MessageBoxResult.No)
            {
               continue;
            }
            else if(res == MessageBoxResult.Cancel)
            {
               break;
            }

            x.Clear();
         }
      }

      // access mode
      private void comboBoxMode_DropDownClosed(object sender, EventArgs e)
      {
         GMaps.Instance.Mode = (AccessMode) comboBoxMode.SelectedItem;
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
            dlg.FileName = "GMapNET Image"; // Default file name
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
         MainMap.Markers.Clear();
         if(checkBoxCurrentMarker.IsChecked.Value)
         {
            MainMap.Markers.Add(currentMarker);
         }
      }

      // add marker
      private void button8_Click(object sender, RoutedEventArgs e)
      {
         GMapMarker m = new GMapMarker(MainMap, MainMap.CurrentPosition);

         Placemark p = null;
         if(checkBoxPlace.IsChecked.Value)
         {
            p = GMaps.Instance.GetPlacemarkFromGeocoder(MainMap.CurrentPosition);
         }

         string ToolTipText;
         if(p != null)
         {
            ToolTipText = p.Address;
         }
         else
         {
            ToolTipText = MainMap.CurrentPosition.ToString();
         }

         m.Shape = new CustomMarkerDemo(m, ToolTipText);

         MainMap.Markers.Add(m);
      }

      // sets route start
      private void button11_Click(object sender, RoutedEventArgs e)
      {
         start = MainMap.CurrentPosition;
      }

      // sets route end
      private void button9_Click(object sender, RoutedEventArgs e)
      {
         end = MainMap.CurrentPosition;
      }

      // adds route
      private void button12_Click(object sender, RoutedEventArgs e)
      {
         List<PointLatLng> route = GMaps.Instance.GetRouteBetweenPoints(start, end, false, MainMap.Zoom);
         if(route != null)
         {
            GMapMarker m1 = new GMapMarker(MainMap, start);
            m1.Shape = new CustomMarkerDemo(m1, "Start: " + start.ToString());

            GMapMarker m2 = new GMapMarker(MainMap, end);
            m2.Shape = new CustomMarkerDemo(m2, "End: " + start.ToString());

            GMapMarker mRoute = new GMapMarker(MainMap, start);
            {
               mRoute.Route.AddRange(route);
               mRoute.RegenerateRouteShape();
               mRoute.ZIndex = -1;
            }

            MainMap.Markers.Add(m1);
            MainMap.Markers.Add(m2);
            MainMap.Markers.Add(mRoute);
         }
      }
   }
}
