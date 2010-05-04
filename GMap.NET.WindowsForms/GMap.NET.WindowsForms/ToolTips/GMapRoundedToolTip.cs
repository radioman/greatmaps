namespace GMap.NET.WindowsForms
{
    using System.Drawing;
    using System.Drawing.Drawing2D;

    /// <summary>
    /// GMap.NET marker
    /// </summary>
    public class GMapRoundedToolTip : GMapToolTip
    {
        public GMapRoundedToolTip(GMapMarker marker) : base(marker)
        {
            
        }
        public void RoundRectangle(Graphics objG, Pen objP, float h, float v, float width, float height, float radius)
        {
            GraphicsPath objGP = new GraphicsPath(); objGP.AddLine(h + radius, v, h + width - (radius * 2), v); objGP.AddArc(h + width - (radius * 2), v, radius * 2, radius * 2, 270, 90); objGP.AddLine(h + width, v + radius, h + width, v + height - (radius * 2)); objGP.AddArc(h + width - (radius * 2), v + height - (radius * 2), radius * 2, radius * 2, 0, 90); // Corner
            objGP.AddLine(h + width - (radius * 2), v + height, h + radius, v + height);
            objGP.AddArc(h, v + height - (radius * 2), radius * 2, radius * 2, 90, 90);
            objGP.AddLine(h, v + height - (radius * 2), h, v + radius);
            objGP.AddArc(h, v, radius * 2, radius * 2, 180, 90);
            objGP.CloseFigure();
            objG.DrawPath(objP, objGP);
            objGP.Dispose();
        }

        public override void DrawToolTip(Graphics g, GMapMarker m, int x, int y)
        {
#if !PocketPC
            GraphicsState s = g.Save();
            g.SmoothingMode = SmoothingMode.AntiAlias;
#endif

            System.Drawing.Size st = g.MeasureString(ToolTipText, ToolTipFont).ToSize();
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(x, y, st.Width + Marker.Overlay.Control.TooltipTextPadding.Width, st.Height + Marker.Overlay.Control.TooltipTextPadding.Height);
            rect.Offset(ToolTipOffset.X, ToolTipOffset.Y);

            g.DrawLine(ToolTipPen, x, y, rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
            g.FillRectangle(ToolTipBackground, rect);
            RoundRectangle(g, ToolTipPen, rect.X, rect.Y, rect.Width, rect.Height, 10f);
#if !PocketPC
            g.DrawString(ToolTipText, ToolTipFont, Brushes.Navy, rect, ToolTipFormat);
#else
            g.DrawString(ToolTipText, ToolTipFont, TooltipForeground, rect, ToolTipFormat);
#endif

#if !PocketPC
            g.Restore(s);
#endif
        }
    }
}
