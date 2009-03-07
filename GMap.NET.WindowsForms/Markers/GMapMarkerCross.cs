using System.Drawing;

namespace GMapNET
{
   public class GMapMarkerCross : GMapMarker
   {
      public Pen Pen;

      public GMapMarkerCross(PointLatLng p) : base(p)
      {
         Pen = new Pen(Brushes.Red, 1);
      }

      public override void OnRender(Graphics g)
      {
         System.Drawing.Point p1 = new System.Drawing.Point(LocalPosition.X, LocalPosition.Y);
         p1.Offset(0, -10);
         System.Drawing.Point p2 = new System.Drawing.Point(LocalPosition.X, LocalPosition.Y);
         p2.Offset(0, 10);

         System.Drawing.Point p3 = new System.Drawing.Point(LocalPosition.X, LocalPosition.Y);
         p3.Offset(-10, 0);
         System.Drawing.Point p4 = new System.Drawing.Point(LocalPosition.X, LocalPosition.Y);
         p4.Offset(10, 0);

         g.DrawLine(Pen, p1, p2);
         g.DrawLine(Pen, p3, p4);
      }
   }
}
