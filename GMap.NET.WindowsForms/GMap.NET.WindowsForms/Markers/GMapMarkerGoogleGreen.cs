
namespace GMap.NET.WindowsForms.Markers
{
   using System.Drawing;

#if !PocketPC
   using System.Windows.Forms.Properties;
#else
   using GMap.NET.WindowsMobile.Properties;
#endif

   public class GMapMarkerGoogleGreen : GMapMarker
   {
      public GMapMarkerGoogleGreen(PointLatLng p)
         : base(p)
      {

      }

      public override void OnRender(Graphics g)
      {
#if !PocketPC
         g.DrawImageUnscaled(Resources.shadow50, LocalPosition.X-10, LocalPosition.Y-34);
         g.DrawImageUnscaled(Resources.bigMarkerGreen, LocalPosition.X-10, LocalPosition.Y-34);
#else
            DrawImageUnscaled(g, Resources.shadow50, LocalPosition.X - 10, LocalPosition.Y - 34);
            DrawImageUnscaled(g, Resources.marker, LocalPosition.X - 10, LocalPosition.Y - 34);
#endif
      }
   }
}
