
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
         if(!IsDragging)
         {
#if !PocketPC
            g.DrawImageUnscaled(Resources.shadow50, LocalPosition.X-10, LocalPosition.Y-34);
            g.DrawImageUnscaled(Resources.bigMarkerGreen, LocalPosition.X-10, LocalPosition.Y-34);
#else
            DrawImageUnscaled(g, Resources.shadow50, LocalPosition.X - 10, LocalPosition.Y - 34);
            DrawImageUnscaled(g, Resources.marker, LocalPosition.X - 10, LocalPosition.Y - 34);
#endif
         }
         else
         {
#if !PocketPC
            g.DrawImageUnscaled(Resources.shadow50, LocalPosition.X-10, LocalPosition.Y-40);
            g.DrawImageUnscaled(Resources.bigMarkerGreen, LocalPosition.X-10, LocalPosition.Y-40);
            g.DrawImageUnscaled(Resources.drag_cross_67_16, LocalPosition.X-8, LocalPosition.Y-8);
#else
            DrawImageUnscaled(g, Resources.shadow50, LocalPosition.X-10, LocalPosition.Y-40);
            DrawImageUnscaled(g, Resources.bigMarkerGreen, LocalPosition.X-10, LocalPosition.Y-40);
            DrawImageUnscaled(g, Resources.drag_cross_67_16, LocalPosition.X-8, LocalPosition.Y-8);
#endif
         }
      }
   }
}
