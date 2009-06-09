
namespace GMap.NET.WindowsPresentation
{
   using System.Collections.Generic;
   using System.ComponentModel;
   using System.Windows;
   using System.Windows.Controls;
   using GMap.NET;

   /// <summary>
   /// GMap.NET marker
   /// </summary>
   public class GMapMarker : INotifyPropertyChanged
   {
      public event PropertyChangedEventHandler PropertyChanged;
      void OnPropertyChanged(string name)
      {
         if(PropertyChanged != null)
         {
            PropertyChanged(this, new PropertyChangedEventArgs(name));
         }
      }

      UIElement shape;

      /// <summary>
      /// marker visual
      /// </summary>
      public UIElement Shape
      {
         get
         {
            return shape;
         }
         set
         {
            if(shape != value)
            {
               shape = value;
               OnPropertyChanged("Shape");
            }
         }
      }

      private PointLatLng position;

      /// <summary>
      /// coordinate of marker
      /// </summary>
      public PointLatLng Position
      {
         get
         {
            return position;
         }
         set
         {
            if(position != value)
            {
               position = value; 
               UpdateLocalPosition();
            }
         }
      }

      /// <summary>
      /// custom object
      /// </summary>
      public object Tag;

      System.Windows.Point offset;
      /// <summary>
      /// offset of marker
      /// </summary>
      public System.Windows.Point Offset
      {
         get
         {
            return offset;
         }
         set
         {
            if(offset != value)
            {
               offset = value;
               UpdateLocalPosition();
            }
         }
      }

      int localPositionX;

      /// <summary>
      /// local X position of marker
      /// </summary>
      public int LocalPositionX
      {
         get
         {
            return localPositionX;
         }
         internal set
         {
            if(localPositionX != value)
            {
               localPositionX = value;
               OnPropertyChanged("LocalPositionX");
            }
         }
      }

      int localPositionY;

      /// <summary>
      /// local Y position of marker
      /// </summary>
      public int LocalPositionY
      {
         get
         {
            return localPositionY;
         }
         internal set
         {
            if(localPositionY != value)
            {
               localPositionY = value;
               OnPropertyChanged("LocalPositionY");
            }
         }
      }

      int zIndex;

      /// <summary>
      /// the index of Z, render order
      /// </summary>
      public int ZIndex
      {
         get
         {
            return zIndex;
         }
         set
         {
            if(zIndex != value)
            {
               zIndex = value;
               OnPropertyChanged("ZIndex");
            }
         }
      }

      /// <summary>
      /// if marker is a route that is a path of it's coordinates
      /// </summary>
      public readonly List<PointLatLng> Route = new List<PointLatLng>();

      internal GMapControl Control;

      public GMapMarker(GMapControl control, PointLatLng pos)
      {
         Control = control;
         Position = pos;
      }

      /// <summary>
      /// sets shape to null and clears route
      /// </summary>
      public void Clear()
      {
         Shape = null;
         Route.Clear();
      }

      /// <summary>
      /// updates marker position, internal only
      /// </summary>
      internal void UpdateLocalPosition()
      {
         GMap.NET.Point p = Control.FromLatLngToLocal(Position);

         LocalPositionX = p.X + (int)Offset.X;
         LocalPositionY = p.Y + (int)Offset.Y;
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