using System.Drawing;

namespace GMapNET
{
   public class GMapMarkerRect : GMapMarker
   {
      public Pen Pen;
      public Size Size;

      public GMapMarkerRect(PointLatLng p) : base(p)
      {
         Pen = new Pen(Brushes.Red, 5);
         Size = new Size(100, 100);
      }

      public override void OnRender(Graphics g)
      {
         g.DrawRectangle(Pen, new System.Drawing.Rectangle(LocalPosition.X-Size.Width/2, LocalPosition.Y-Size.Height/2, Size.Width, Size.Height));
      }
   }
}
