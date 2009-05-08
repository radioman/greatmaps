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
            LocalPosition = new Point(p.X, p.Y);
         }
      }

      public object Tag;

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

      internal GMap Control;

      public GMapMarker(GMap control, PointLatLng pos)
      {
         Control = control;
         Position = pos;
      }
   } 
}