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
         GMaps.Instance.Timeout = 10*1000;

         // set your proxy here if need
         //GMaps.Instance.Proxy = new WebProxy("10.2.0.100", 8080);
         //GMaps.Instance.Proxy.Credentials = new NetworkCredential("ogrenci@bilgeadam.com", "bilgeadam");

         // config map
         MainMap.MapType = GMapType.OpenStreetMap;
         MainMap.Zoom = 13;
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

      // on form load
      private void Window_Loaded(object sender, RoutedEventArgs e)
      {
         MainMap.ReloadMap();
      }

      // tile louading starts
      void MainMap_OnTileLoadStart(int loaderId)
      {
         switch(loaderId)
         {
            case 1:
            progressBar1.IsIndeterminate = true;
            break;

            case 2:
            progressBar2.IsIndeterminate = true;
            break;

            case 3:
            progressBar3.IsIndeterminate = true;
            break;
         }
      }

      // tile loading stops
      void MainMap_OnTileLoadComplete(int loaderId)
      {
         switch(loaderId)
         {
            case 1:
            progressBar1.IsIndeterminate = false;
            break;

            case 2:
            progressBar2.IsIndeterminate = false;
            break;

            case 3:
            progressBar3.IsIndeterminate = false;
            break;
         }
      }

      void MainMap_OnMarkerClick(Marker item)
      {
         //throw new NotImplementedException();
      }

      void MainMap_OnCurrentPositionChanged(PointLatLng point)
      {
         //throw new NotImplementedException();
      }

      // reload
      private void button1_Click(object sender, RoutedEventArgs e)
      {
         MainMap.ReloadMap();
      }

      // map type changed
      private void comboBoxMapType_DropDownClosed(object sender, EventArgs e)
      {
         MainMap.MapType = (GMapType)comboBoxMapType.SelectedItem;
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
            MainMap.ReloadMap();
         }
      }       
   }
}
