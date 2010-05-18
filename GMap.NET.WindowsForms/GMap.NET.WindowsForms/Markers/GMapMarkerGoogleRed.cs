
namespace GMap.NET.WindowsForms.Markers
{
   using System.Drawing;

#if !PocketPC
   using System.Windows.Forms.Properties;
#else
   using GMap.NET.WindowsMobile.Properties;
#endif

   public class GMapMarkerGoogleRed : GMapMarker
   {
      public float? Bearing;

      public GMapMarkerGoogleRed(PointLatLng p)
         : base(p)
      {

      }

      Point[] Arrow = new Point[] { new Point(-5, 5), new Point(0, -20), new Point(5, 5), new Point(0, 2) };

      public override void OnRender(Graphics g)
      {
#if !PocketPC

         g.DrawImageUnscaled(Resources.shadow50, LocalPosition.X-10, LocalPosition.Y-34);

         if(Bearing.HasValue)
         {
            g.TranslateTransform(LocalPosition.X, LocalPosition.Y);
            g.RotateTransform(Bearing.Value);

            g.FillPolygon(Brushes.Red, Arrow);

            g.ResetTransform();
         }

         g.DrawImageUnscaled(Resources.marker, LocalPosition.X-10, LocalPosition.Y-34);
#else
            DrawImageUnscaled(g, Resources.shadow50, LocalPosition.X-10, LocalPosition.Y-34);
            DrawImageUnscaled(g, Resources.marker, LocalPosition.X-10, LocalPosition.Y-34);
#endif
      }
   }
}
