
namespace GMap.NET.WindowsForms.Markers
{
   using System.Drawing;
   using System.Windows.Forms.Properties;

   public class GMapMarkerGoogleRed : GMapMarker
   {
      public GMapMarkerGoogleRed(PointLatLng p) : base(p)
      {
         
      }

      public override void OnRender(Graphics g)
      {
         if(!IsDragging)
         {
            g.DrawImageUnscaled(Resources.shadow50, LocalPosition.X-10, LocalPosition.Y-34);
            g.DrawImageUnscaled(Resources.marker, LocalPosition.X-10, LocalPosition.Y-34);
         }
         else
         {
            g.DrawImageUnscaled(Resources.shadow50, LocalPosition.X-10, LocalPosition.Y-40);
            g.DrawImageUnscaled(Resources.marker, LocalPosition.X-10, LocalPosition.Y-40);
            g.DrawImageUnscaled(Resources.drag_cross_67_16, LocalPosition.X-8, LocalPosition.Y-8);
         }
      }
   }
}
