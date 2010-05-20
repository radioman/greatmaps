namespace GMap.NET.WindowsForms
{
   using System.Drawing;
   using System.Drawing.Drawing2D;

   /// <summary>
   /// GMap.NET marker
   /// </summary>
   public class GMapToolTip
   {
      internal GMapMarker Marker;

      public Point Offset;

      /// <summary>
      /// string format
      /// </summary>
      public StringFormat Format = new StringFormat();

      /// <summary>
      /// font
      /// </summary>
#if !PocketPC
      public Font Font = new Font(FontFamily.GenericSansSerif, 14, FontStyle.Bold, GraphicsUnit.Pixel);
#else
      public Font Font = new Font(FontFamily.GenericSansSerif, 6, FontStyle.Bold);
#endif

      /// <summary>
      /// specifies how the outline is painted
      /// </summary>
#if !PocketPC
      public Pen Stroke = new Pen(Color.FromArgb(140, Color.MidnightBlue));
#else
      public Pen Stroke = new Pen(Color.MidnightBlue);
#endif

      /// <summary>
      /// background color
      /// </summary>
#if !PocketPC
      public Brush Fill = new SolidBrush(Color.FromArgb(222, Color.AliceBlue));
#else
      public Brush Fill = new System.Drawing.SolidBrush(Color.AliceBlue);

      /// <summary>
      /// text foreground
      /// </summary>
      public Brush Foreground = new System.Drawing.SolidBrush(Color.Navy);
#endif

      /// <summary>
      /// text padding
      /// </summary>
      public Size TextPadding = new Size(10, 10);

      public GMapToolTip(GMapMarker marker)
      {
         this.Marker = marker;
         this.Offset = new Point(14, -44);

         this.Stroke.Width = 2;

#if !PocketPC
         this.Stroke.LineJoin = LineJoin.Round;
         this.Stroke.StartCap = LineCap.RoundAnchor;
#endif

         this.Format.Alignment = StringAlignment.Center;
         this.Format.LineAlignment = StringAlignment.Center;
      }

      public virtual void Draw(Graphics g)
      {
         System.Drawing.Size st = g.MeasureString(Marker.ToolTipText, Font).ToSize();
         System.Drawing.Rectangle rect = new System.Drawing.Rectangle(Marker.ToolTipPosition.X, Marker.ToolTipPosition.Y - st.Height, st.Width + TextPadding.Width, st.Height + TextPadding.Height);
         rect.Offset(Offset.X, Offset.Y);

         g.DrawLine(Stroke, Marker.ToolTipPosition.X, Marker.ToolTipPosition.Y, rect.X, rect.Y + rect.Height / 2);

         g.FillRectangle(Fill, rect);
         g.DrawRectangle(Stroke, rect);

#if !PocketPC
         g.DrawString(Marker.ToolTipText, Font, Brushes.Navy, rect, Format);
#else
         g.DrawString(Marker.ToolTipText, Font, Foreground, rect, Format);
#endif
      }
   }
}
