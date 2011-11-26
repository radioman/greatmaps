
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
      public float? Bearing;

      static readonly Bitmap shadow = Resources.shadow50;
      static readonly Bitmap marker = Resources.bigMarkerGreen;
      static readonly System.Drawing.Size SizeSt = new System.Drawing.Size(marker.Width, marker.Height);

      public GMapMarkerGoogleGreen(PointLatLng p)
         : base(p)
      {
         Size = SizeSt;
         Offset = new Point(-10, -34);
      }

      static readonly Point[] Arrow = new Point[] { new Point(-7, 7), new Point(0, -22), new Point(7, 7), new Point(0, 2) };

      public override void OnRender(Graphics g)
      {
#if !PocketPC
         if(!Bearing.HasValue)
         {
            g.DrawImageUnscaled(shadow, LocalPosition.X, LocalPosition.Y);
         }
         else
         {
            //g.TranslateTransform(ToolTipPosition.X, ToolTipPosition.Y); 
            //g.RotateTransform(Bearing.Value - Overlay.Control.Bearing);
            //g.FillPolygon(Brushes.Lime, Arrow);
            //g.ResetTransform();
         }

         if(!Bearing.HasValue)
         {
            g.DrawImageUnscaled(marker, LocalPosition.X, LocalPosition.Y);
         }
#else
            DrawImageUnscaled(g, shadow, LocalPosition.X, LocalPosition.Y);
            DrawImageUnscaled(g, marker, LocalPosition.X, LocalPosition.Y);
#endif
      }
   }
}
