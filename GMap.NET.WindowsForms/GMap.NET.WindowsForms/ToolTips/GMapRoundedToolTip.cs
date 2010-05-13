
namespace GMap.NET.WindowsForms.ToolTips
{
   using System.Drawing;
   using System.Drawing.Drawing2D;

#if !PocketPC
   /// <summary>
   /// GMap.NET marker
   /// </summary>
   public class GMapRoundedToolTip : GMapToolTip
   {
      public float Radius = 10f;

      public GMapRoundedToolTip(GMapMarker marker)
         : base(marker)
      {

      }

      public void DrawRoundRectangle(Graphics g, Pen pen, float h, float v, float width, float height, float radius)
      {
         using(GraphicsPath gp = new GraphicsPath())
         {
            gp.AddLine(h + radius, v, h + width - (radius * 2), v);
            gp.AddArc(h + width - (radius * 2), v, radius * 2, radius * 2, 270, 90);
            gp.AddLine(h + width, v + radius, h + width, v + height - (radius * 2));
            gp.AddArc(h + width - (radius * 2), v + height - (radius * 2), radius * 2, radius * 2, 0, 90); // Corner
            gp.AddLine(h + width - (radius * 2), v + height, h + radius, v + height);
            gp.AddArc(h, v + height - (radius * 2), radius * 2, radius * 2, 90, 90);
            gp.AddLine(h, v + height - (radius * 2), h, v + radius);
            gp.AddArc(h, v, radius * 2, radius * 2, 180, 90);
           
            gp.CloseFigure();

            g.FillPath(Fill, gp);
            g.DrawPath(pen, gp);
         }
      }

      public override void Draw(Graphics g)
      {
         System.Drawing.Size st = g.MeasureString(Marker.ToolTipText, Font).ToSize();
         System.Drawing.Rectangle rect = new System.Drawing.Rectangle(Marker.LocalPosition.X, Marker.LocalPosition.Y - st.Height, st.Width + Marker.Overlay.Control.TooltipTextPadding.Width, st.Height + Marker.Overlay.Control.TooltipTextPadding.Height);
         rect.Offset(Offset.X, Offset.Y);

         g.DrawLine(Stroke, Marker.LocalPosition.X, Marker.LocalPosition.Y, rect.X + Radius/2, rect.Y + rect.Height - Radius/2);

         DrawRoundRectangle(g, Stroke, rect.X, rect.Y, rect.Width, rect.Height, Radius);

#if !PocketPC
         g.DrawString(Marker.ToolTipText, Font, Brushes.Navy, rect, Format);
#else
         g.DrawString(ToolTipText, ToolTipFont, TooltipForeground, rect, ToolTipFormat);
#endif
      }
   }
#endif
}
