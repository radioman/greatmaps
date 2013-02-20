using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using GMap.NET.WindowsPresentation;
using GMap.NET.MapProviders;

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

         Map.SetPositionByKeywords(Location);
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
            map.MapProvider = GMapProviders.OpenStreetMap;
            map.MinZoom = 4;
            map.MaxZoom = map.MinZoom + 3;
            map.Zoom = map.MinZoom;
            mapCtrl.Add(new MapArr(map, "Holand"));
         }

         // 2
         {
            GMapControl map = new GMapControl();
            map.MapProvider = GMapProviders.OpenStreetMap;
            map.MinZoom = 4;
            map.MaxZoom = map.MinZoom + 3;
            map.Zoom = map.MinZoom;
            mapCtrl.Add(new MapArr(map, "New York"));
         }

         // 3
         {
            GMapControl map = new GMapControl();
            map.MapProvider = GMapProviders.OpenStreetMap;
            map.MinZoom = 4;
            map.MaxZoom = map.MinZoom + 3;
            map.Zoom = map.MinZoom;
            mapCtrl.Add(new MapArr(map, "Lithuania"));
         }

         // main
         UserMap.MapProvider = GMapProviders.GoogleMap;
         UserMap.MinZoom = 5;
         UserMap.MaxZoom = 13;
         UserMap.Zoom = 5;
         UserMap.SetPositionByKeywords("Leuven");

         // add all maps
         locations.ItemsSource = mapCtrl;
      }
   }
}
