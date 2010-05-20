
namespace Demo.WindowsForms.CustomMarkers
{
   using System.Drawing;
   using GMap.NET.WindowsForms;
   using GMap.NET.WindowsForms.Markers;
   using GMap.NET;

   public class GMapMarkerRect : GMapMarker
   {
      public Pen Pen;

      public GMapMarkerGoogleGreen InnerMarker;

      public GMapMarkerRect(PointLatLng p)
         : base(p)
      {
         Pen = new Pen(Brushes.Blue, 5);

         // do not forget set Size of the marker
         // if so, you shall have no event on it ;}
         Size = new System.Drawing.Size(111, 111);
         Offset = new System.Drawing.Point(-Size.Width/2, -Size.Height/2); 
      }

      public override void OnRender(Graphics g)
      {
         g.DrawRectangle(Pen, new System.Drawing.Rectangle(LocalPosition.X, LocalPosition.Y, Size.Width, Size.Height));
      }
   }
}
