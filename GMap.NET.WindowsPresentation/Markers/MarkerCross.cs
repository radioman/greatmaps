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
         Point p1 = LocalPosition;
         p1.Offset(0, -10);
         Point p2 = LocalPosition;
         p2.Offset(0, 10);

         Point p3 = LocalPosition;
         p3.Offset(-10, 0);
         Point p4 = LocalPosition;
         p4.Offset(10, 0);

         g.DrawLine(Pen, p1, p2);
         g.DrawLine(Pen, p3, p4);
      }
   }
}
