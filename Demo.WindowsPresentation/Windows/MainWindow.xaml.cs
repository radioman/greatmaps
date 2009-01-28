using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Net;

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
      }

      // on form load
      private void Window_Loaded(object sender, RoutedEventArgs e)
      {
         MainMap.ReloadMap();
      }

      void MainMap_OnTileLoadStart(int loaderId)
      {
         //throw new NotImplementedException();
      }

      void MainMap_OnTileLoadComplete(int loaderId)
      {
         //throw new NotImplementedException();
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
   }
}
