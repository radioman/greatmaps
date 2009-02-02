using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows.Threading;
using System.Net;
using System.Globalization;

using GMapNET;

namespace Demo.WindowsPresentation
{
   public partial class MainWindow : Window
   {
      public MainWindow()
      {
         InitializeComponent();

         // config form and add map
         this.Background = Brushes.AliceBlue;

         // config gmaps
         GMaps.Instance.Language = "lt";
         GMaps.Instance.UseTileCache = true;
         GMaps.Instance.UseRouteCache = true;
         GMaps.Instance.UseGeocoderCache = true;
         GMaps.Instance.UsePlacemarkCache = true;
         GMaps.Instance.Mode = AccessMode.ServerAndCache;

         // set your proxy here if need
         //GMaps.Instance.Proxy = new WebProxy("10.2.0.100", 8080);
         //GMaps.Instance.Proxy.Credentials = new NetworkCredential("ogrenci@bilgeadam.com", "bilgeadam");

         // config map
         MainMap.MapType = GMapType.OpenStreetMap;
         MainMap.Zoom = 12;
         MainMap.CurrentMarkerEnabled = true;
         MainMap.CurrentMarkerStyle = CurrentMarkerType.GMap;
         MainMap.CurrentPosition = new PointLatLng(54.6961334816182, 25.2985095977783);

         // map events
         MainMap.OnCurrentPositionChanged += new CurrentPositionChanged(MainMap_OnCurrentPositionChanged);
         MainMap.OnMarkerClick += new MarkerClick(MainMap_OnMarkerClick);
         MainMap.OnTileLoadComplete += new TileLoadComplete(MainMap_OnTileLoadComplete);
         MainMap.OnTileLoadStart += new TileLoadStart(MainMap_OnTileLoadStart);

         // get map types
         comboBoxMapType.ItemsSource = Enum.GetValues(typeof(GMapType));
         comboBoxMapType.SelectedItem = MainMap.MapType;

         // acccess mode
         comboBoxMode.ItemsSource = Enum.GetValues(typeof(AccessMode));
         comboBoxMode.SelectedItem = GMaps.Instance.Mode;

         // get cache modes
         checkBoxCacheMap.IsChecked = GMaps.Instance.UseTileCache;
         checkBoxCacheRoute.IsChecked = GMaps.Instance.UseRouteCache;
         checkBoxGeoCache.IsChecked = GMaps.Instance.UseGeocoderCache;

         // get position
         textBoxLat.Text = MainMap.CurrentPosition.Lat.ToString(CultureInfo.InvariantCulture);
         textBoxLng.Text = MainMap.CurrentPosition.Lng.ToString(CultureInfo.InvariantCulture);

         // get marker state
         checkBoxCurrentMarker.IsChecked = MainMap.CurrentMarkerEnabled;

         // can drag map
         checkBoxDragMap.IsChecked = MainMap.CanDragMap;

         // get zoom
         sliderZoom.Maximum = GMaps.Instance.MaxZoom;
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

      void MainMap_OnMarkerClick(Marker item)
      {
         //throw new NotImplementedException();
      }

      // current location changed
      void MainMap_OnCurrentPositionChanged(PointLatLng point)
      {
         textBoxCurrLat.Text = point.Lat.ToString(CultureInfo.InvariantCulture);
         textBoxCurrLng.Text = point.Lng.ToString(CultureInfo.InvariantCulture);
      }

      // reload
      private void button1_Click(object sender, RoutedEventArgs e)
      {
         MainMap.ReloadMap();
      }

      // map type changed
      private void comboBoxMapType_DropDownClosed(object sender, EventArgs e)
      {
         MainMap.MapType = (GMapType) comboBoxMapType.SelectedItem;
         MainMap.ReloadMap();
      }

      // enable current marker
      private void checkBoxCurrentMarker_Checked(object sender, RoutedEventArgs e)
      {
         MainMap.CurrentMarkerEnabled = true;
      }

      // disable current marker
      private void checkBoxCurrentMarker_Unchecked(object sender, RoutedEventArgs e)
      {
         MainMap.CurrentMarkerEnabled = false;
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
         if(zn != MainMap.Zoom)
         {
            MainMap.Zoom = zn;
         }
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
               Prefetch obj = new Prefetch();
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

      // use tile cache
      private void checkBox_CacheChecked(object sender, RoutedEventArgs e)
      {
         GMaps.Instance.UseTileCache = checkBoxCacheMap.IsChecked.Value;
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
   }
}
