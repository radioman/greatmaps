using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;

namespace TemplatedBinding
{
   class MapArr : INotifyPropertyChanged
   {
      public event PropertyChangedEventHandler PropertyChanged;
      void OnPropertyChanged(string name)
      {
         if(PropertyChanged != null)
         {
            PropertyChanged(this, new PropertyChangedEventArgs(name));
         }
      }

      GMapControl map;
      public GMapControl Map
      {
         get
         {
            return map;
         }
         set
         {
            map = value;
            OnPropertyChanged("Map");
         }
      }

      string location;
      public string Location
      {
         get
         {
            return location;
         }
         set
         {
            location = value;
            OnPropertyChanged("Location");
         }
      }

      public MapArr(GMapControl m, string location)
      {
         Map = m;
         Location = location;

         Map.SetCurrentPositionByKeywords(Location);
      }
   }

   /// <summary>
   /// Interaction logic for Window1.xaml
   /// </summary>
   public partial class Window1 : Window
   {
      public Window1()
      {
         InitializeComponent();

         ObservableCollection<MapArr> mapCtrl = new ObservableCollection<MapArr>();

         // 1
         {
            GMapControl map = new GMapControl();
            map.MapType = GMap.NET.MapType.OpenStreetMap;
            map.MinZoom = 4;
            map.MaxZoom = map.MinZoom + 3;
            map.Zoom = map.MinZoom;
            mapCtrl.Add(new MapArr(map, "Holand"));
         }

         // 2
         {
            GMapControl map = new GMapControl();
            map.MapType = GMap.NET.MapType.VirtualEarthMap;
            map.MinZoom = 4;
            map.MaxZoom = map.MinZoom + 3;
            map.Zoom = map.MinZoom;
            mapCtrl.Add(new MapArr(map, "New York"));
         }

         // 3
         {
            GMapControl map = new GMapControl();
            map.MapType = GMap.NET.MapType.YahooMap;
            map.MinZoom = 4;
            map.MaxZoom = map.MinZoom + 3;
            map.Zoom = map.MinZoom;
            mapCtrl.Add(new MapArr(map, "Lithuania"));
         }

         // main
         UserMap.MapType = GMap.NET.MapType.GoogleMap;
         UserMap.MinZoom = 5;
         UserMap.MaxZoom = 13;
         UserMap.Zoom = 5;
         UserMap.SetCurrentPositionByKeywords("Leuven");

         // add all maps
         locations.ItemsSource = mapCtrl;
      }
   }
}
