using System.Windows;
using System.Windows.Media;   

namespace GMapNET
{
   public class MarkerCross : MapObject
   {
      public Pen Pen;
      public Point LocalPosition;

      public MarkerCross()
      {
         Pen = new Pen(Brushes.Red, 1);
      }

      public void OnRender(DrawingContext g)
      {
         System.Windows.Point p1 = new System.Windows.Point(LocalPosition.X, LocalPosition.Y);
         p1.Offset(0, -10);
         System.Windows.Point p2 = new System.Windows.Point(LocalPosition.X, LocalPosition.Y);
         p2.Offset(0, 10);

         System.Windows.Point p3 = new System.Windows.Point(LocalPosition.X, LocalPosition.Y);
         p3.Offset(-10, 0);
         System.Windows.Point p4 = new System.Windows.Point(LocalPosition.X, LocalPosition.Y);
         p4.Offset(10, 0);

         g.DrawLine(Pen, p1, p2);
         g.DrawLine(Pen, p3, p4);
      }
   }
}
