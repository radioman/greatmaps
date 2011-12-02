using System;
using System.Windows;
using System.Diagnostics;
using System.Threading;
using GMap.NET.MapProviders;

namespace Sample3
{
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window
   {
      public MainWindow()
      {
         InitializeComponent();

         MainMap.MapProvider = GMapProviders.GoogleMap;
         MapArcGISworldTopo.MapProvider = GMapProviders.ArcGIS_World_Topo_Map;

         MapBing.MapProvider = GMapProviders.BingMap;
         MapBingHybrid.MapProvider = GMapProviders.BingHybridMap;
         MapBingOld.MapProvider = GMapProviders.BingMapOld;

         MapGoogleHybrid.MapProvider = GMapProviders.GoogleHybridMap;
         MapGoogleTerrain.MapProvider = GMapProviders.GoogleTerrainMap;

         MapYahoo.MapProvider = GMapProviders.YahooMap;
         MapYahooHybrid.MapProvider = GMapProviders.YahooHybridMap;

         MapYandexMapRuHybrid.MapProvider = GMapProviders.YandexHybridMap;
         MapYandexRu.MapProvider = GMapProviders.YandexMap;

         MapLt.MapProvider = GMapProviders.LithuaniaMap;
         MapLtHybrid.MapProvider = GMapProviders.LithuaniaOrtoFotoMap;

         MapOpenStreet.MapProvider = GMapProviders.OpenStreetMap;
         MapOpenStreetOsm.MapProvider = GMapProviders.OpenStreetOsm;
         MapOpenStreetSurfer.MapProvider = GMapProviders.OpenStreetMapSurfer;

         this.WindowState = System.Windows.WindowState.Minimized;
      }

      private void OnMapZoomChanged()
      {
         this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
         {
            var amap = dockManager.ActiveContent.Content as GMap.NET.WindowsPresentation.GMapControl;
            if(amap != null)
            {
               foreach(var d in dockManager.Documents)
               {
                  var map = d.Content as GMap.NET.WindowsPresentation.GMapControl;

                  if(map != null && map != amap)
                  {
                     if(map.MapProvider.Projection.ToString() == amap.MapProvider.Projection.ToString())
                     {
                        map.Zoom = amap.Zoom;
                     }
                     else
                     {
                        map.SetZoomToFitRect(amap.ViewArea);
                     }
                  }
               }
            }
         }));
      }

      private void OnMapDrag()
      {
         this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(delegate()
         {
            var amap = dockManager.ActiveContent.Content as GMap.NET.WindowsPresentation.GMapControl;
            if(amap != null)
            {
               last = amap.Position;

               foreach(var d in dockManager.Documents)
               {
                  var map = d.Content as GMap.NET.WindowsPresentation.GMapControl;

                  if(map != null && map != amap)
                  {
                     map.Position = amap.Position;
                  }
               }
            }
         }));
      }

      GMap.NET.PointLatLng last = new GMap.NET.PointLatLng();
      GMap.NET.WindowsPresentation.GMapControl lastMap;

      private void DocumentPane_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
      {
         if(e.RemovedItems.Count > 0 && e.AddedItems.Count > 0)
         {
            var befCont = (e.RemovedItems[0] as AvalonDock.DocumentContent).Content;
            if(befCont != null)
            {
               var before = befCont as GMap.NET.WindowsPresentation.GMapControl;
               if(before != null)
               {
                  var aCont = (e.AddedItems[0] as AvalonDock.DocumentContent).Content;
                  if(aCont != null)
                  {
                     var amap = aCont as GMap.NET.WindowsPresentation.GMapControl;
                     if(amap != null)
                     {
                        //this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
                        //{                   

                        amap.Position = before.Position;

                        if(amap.MapProvider.Projection.ToString() == before.MapProvider.Projection.ToString())
                        {
                           amap.Zoom = before.Zoom;
                        }
                        else
                        {
                           amap.SetZoomToFitRect(before.ViewArea);
                        }
                        //}));
                     }

                     lastMap = amap;
                  }
               }
            }
         }
      }

      private void GMapControl_Loaded(object sender, RoutedEventArgs e)
      {
         var map = sender as GMap.NET.WindowsPresentation.GMapControl;

         if(lastMap != null && lastMap != map)
         {
            map.Position = lastMap.Position;

            if(map.MapProvider.Projection.ToString() == lastMap.MapProvider.Projection.ToString())
            {
               map.Zoom = lastMap.Zoom;
            }
            else
            {
               map.SetZoomToFitRect(lastMap.ViewArea);
            }
         }
         else
         {
            map.Position = last;
         }
      }
   }
}
