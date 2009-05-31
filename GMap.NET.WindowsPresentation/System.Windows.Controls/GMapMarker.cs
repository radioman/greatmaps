using System.ComponentModel;
using GMapNET;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Windows.Controls
{
   public class GMapMarker : INotifyPropertyChanged
   {
      public event PropertyChangedEventHandler PropertyChanged;
      public void OnPropertyChanged(string name)
      {
         PropertyChangedEventHandler handler = PropertyChanged;
         if(handler != null)
         {
            handler(this, new PropertyChangedEventArgs(name));
         }
      }

      UIElement shape;
      public UIElement Shape
      {
         get
         {
            return shape;
         }
         set
         {
            shape = value;
            OnPropertyChanged("Shape");
         }
      }

      private PointLatLng position;
      public PointLatLng Position
      {
         get
         {
            return position;
         }
         set
         {
            position = value;

            GMapNET.Point p = Control.FromLatLngToLocal(value);
            Point pl = new Point(p.X, p.Y);
            pl.Offset(Offset.X, Offset.Y);
            LocalPosition = pl;
         }
      }

      public object Tag;

      Point offset;
      public Point Offset
      {
         get
         {
            return offset;
         }
         set
         {
            offset = value;
            Position = Position;
         }
      }

      Point localPosition;
      public Point LocalPosition
      {
         get
         {
            return localPosition;
         }
         internal set
         {
            localPosition = value;
            OnPropertyChanged("LocalPosition");
         }
      }

      int zIndex;
      public int ZIndex
      {
         get
         {
            return zIndex;
         }
         set
         {
            zIndex = value;
            OnPropertyChanged("ZIndex");
         }
      }

      /// <summary>
      /// if marker is a route that is a path of it's coordinates
      /// </summary>
      public readonly List<PointLatLng> Route = new List<PointLatLng>();

      internal GMap Control;

      public GMapMarker(GMap control, PointLatLng pos)
      {
         Control = control;
         Position = pos;
      }

      /// <summary>
      /// sets shape to null and clears route
      /// </summary>
      public void Clear()
      {
         if(PropertyChanged != null)
         {
            PropertyChanged = null;
         }
         Shape = null;
         Route.Clear();            
      }

      /// <summary>
      /// regenerates shape of route
      /// </summary>
      public void RegenerateRouteShape()
      {
         if(Route.Count > 1)
         {
            var localPath = new List<System.Windows.Point>();
            var offset = Control.FromLatLngToLocal(Route[0]);
            foreach(var i in Route)
            {
               var p = Control.FromLatLngToLocal(new PointLatLng(i.Lat, i.Lng));
               localPath.Add(new System.Windows.Point(p.X - offset.X, p.Y - offset.Y));
            }
            this.Shape = Control.CreateRoutePath(localPath);
         }
      }
   }
}