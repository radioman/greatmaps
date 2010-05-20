
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
         Size = new System.Drawing.Size(Resources.marker.Width, Resources.marker.Height);
         Offset = new Point(-10, -34);
      }

      static readonly Point[] Arrow = new Point[] { new Point(-5, 5), new Point(0, -20), new Point(5, 5), new Point(0, 2) };

      public override void OnRender(Graphics g)
      {
#if !PocketPC

         g.DrawImageUnscaled(Resources.shadow50, LocalPosition.X, LocalPosition.Y);

         if(Bearing.HasValue)
         {
            g.TranslateTransform(ToolTipPosition.X, ToolTipPosition.Y);
            g.RotateTransform(Bearing.Value);

            g.FillPolygon(Brushes.Red, Arrow);

            g.ResetTransform();
         }

         g.DrawImageUnscaled(Resources.marker, LocalPosition.X, LocalPosition.Y);
#else
            DrawImageUnscaled(g, Resources.shadow50, LocalPosition.X, LocalPosition.Y);
            DrawImageUnscaled(g, Resources.marker, LocalPosition.X, LocalPosition.Y);
#endif
      }
   }
}
