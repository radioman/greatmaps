using System.ComponentModel;
using GMapNET;

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

      internal GMap Control;

      public GMapMarker(GMap control, PointLatLng pos)
      {
         Control = control;
         Position = pos;
      }
   } 
}