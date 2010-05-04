namespace GMap.NET.WindowsForms
{
    using System.Drawing;
    using System.Drawing.Drawing2D;

    /// <summary>
    /// GMap.NET marker
    /// </summary>
    public class GMapBaloonToolTip : GMapToolTip
    {
        public float Radius = 10f;

        public GMapBaloonToolTip(GMapMarker marker)
            : base(marker)
        {
            ToolTipPen = new Pen(Color.FromArgb(140, Color.Black));
            ToolTipPen.Width = 3;
#if !PocketPC
            this.ToolTipPen.LineJoin = LineJoin.Round;
            this.ToolTipPen.StartCap = LineCap.RoundAnchor;
#endif

            ToolTipBackground = Brushes.Yellow;
        }

        public void RoundRectangle(Graphics objG, Pen objP, float h, float v, float width, float height, float radius)
        {
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

            GraphicsPath objGP = new GraphicsPath();
            objGP.AddLine(rect.X + 2 * Radius, rect.Y + rect.Height, rect.X + Radius, rect.Y + rect.Height + Radius);
            objGP.AddLine(rect.X + Radius, rect.Y + rect.Height + Radius, rect.X + Radius, rect.Y + rect.Height);

            objGP.AddArc(rect.X, rect.Y + rect.Height - (Radius * 2), Radius * 2, Radius * 2, 90, 90);
            objGP.AddLine(rect.X, rect.Y + rect.Height - (Radius * 2), rect.X, rect.Y + Radius);
            objGP.AddArc(rect.X, rect.Y, Radius * 2, Radius * 2, 180, 90);
            objGP.AddLine(rect.X + Radius, rect.Y, rect.X + rect.Width - (Radius * 2), rect.Y);
            objGP.AddArc(rect.X + rect.Width - (Radius * 2), rect.Y, Radius * 2, Radius * 2, 270, 90);
            objGP.AddLine(rect.X + rect.Width, rect.Y + Radius, rect.X + rect.Width, rect.Y + rect.Height - (Radius * 2));
            objGP.AddArc(rect.X + rect.Width - (Radius * 2), rect.Y + rect.Height - (Radius * 2), Radius * 2, Radius * 2, 0, 90); // Corner
            objGP.CloseFigure();
            g.DrawPath(ToolTipPen, objGP);
            g.FillPath(ToolTipBackground, objGP);
            objGP.Dispose();

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
