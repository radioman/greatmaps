
namespace GMap.NET.WindowsPresentation
{
   using System.Collections.Generic;
   using System.ComponentModel;
   using System.Windows;
   using System.Windows.Controls;
   using GMap.NET;
   using System.Windows.Media;
   using System.Diagnostics;
   using System.Windows.Shapes;
   using System;

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

               UpdateLocalPosition();
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

      GMapControl map;

      /// <summary>
      /// the map of this marker
      /// </summary>
      public GMapControl Map
      {
         get
         {
            if(Shape != null && map == null)
            {
               DependencyObject visual = Shape;
               while(visual != null && !(visual is GMapControl))
               {
                  visual = VisualTreeHelper.GetParent(visual);
               }

               map = visual as GMapControl;
            }

            return map;
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

      public GMapMarker(PointLatLng pos)
      {
         Position = pos;
      }

      public GMapMarker()
      {
         Position = new PointLatLng();
      }

      /// <summary>
      /// calls Dispose on shape if it implements IDisposable, sets shape to null and clears route
      /// </summary>
      public void Clear()
      {
         var s = (Shape as IDisposable);
         if(s != null)
         {
            s.Dispose();
            s = null;
         }
         Shape = null;
         Route.Clear();
         Route.TrimExcess();
      }

      /// <summary>
      /// updates marker position, internal access usualy
      /// </summary>
      internal void UpdateLocalPosition()
      {
         if(Map != null)
         {
            GMap.NET.Point p = Map.FromLatLngToLocal(Position);

            LocalPositionX = p.X + (int) Offset.X;
            LocalPositionY = p.Y + (int) Offset.Y;
         }
      }

      /// <summary>
      /// forces to  update local marker  position
      /// </summary>
      /// <param name="m"></param>
      public void ForceUpdateLocalPosition(GMapControl m)
      {
         if(m != null)
         {
            map = m;
         }
         UpdateLocalPosition();
      }

      /// <summary>
      /// regenerates shape of route
      /// </summary>
      public void RegenerateRouteShape(GMapControl map)
      {
         this.map = map;

         if(map != null && Route.Count > 1)
         {
            var localPath = new List<System.Windows.Point>();
            var offset = Map.FromLatLngToLocal(Route[0]);
            foreach(var i in Route)
            {
               var p = Map.FromLatLngToLocal(new PointLatLng(i.Lat, i.Lng));
               localPath.Add(new System.Windows.Point(p.X - offset.X, p.Y - offset.Y));
            }

            var shape = map.CreateRoutePath(localPath);

            if(this.Shape != null && this.Shape is Path)
            {
               (this.Shape as Path).Data = shape.Data;
            }
            else
            {
               this.Shape = shape;
            }
         }
      }
   }
}