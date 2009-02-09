using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GMapNET;

namespace Demo.WindowsPresentation
{
   public class MarkerDot : MapObject
   {
      public Pen Pen;
      public double Radius = 5;

      public MarkerDot()
      {
         Pen = new Pen(Brushes.Red, 2.0);
      }

      public void OnRender(DrawingContext g)
      {
         System.Windows.Point p = new System.Windows.Point(LocalPosition.X, LocalPosition.Y);
         g.DrawEllipse(Brushes.Red, Pen, p, Radius, Radius);
      }
   }
}
