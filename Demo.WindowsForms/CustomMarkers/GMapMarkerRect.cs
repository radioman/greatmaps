
namespace GMap.NET.WindowsForms.Markers
{
   using System.Drawing;
   using GMap.NET.WindowsForms;

   public class GMapMarkerRect : GMapMarker
   {
      public Pen Pen;

      public GMapMarkerRect(PointLatLng p)
         : base(p)
      {
         Pen = new Pen(Brushes.Red, 5);

         // do not forget set Size of the marker
         // if so, you shall have no event on it ;}
         Size = new Size(55, 55); 
      }

      public override void OnRender(Graphics g)
      {
         g.DrawRectangle(Pen, new System.Drawing.Rectangle(LocalPosition.X - Size.Width / 2, LocalPosition.Y - Size.Height / 2, Size.Width, Size.Height));
      }
   }
}
