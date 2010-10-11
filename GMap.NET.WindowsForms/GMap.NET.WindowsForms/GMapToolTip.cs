
namespace GMap.NET.WindowsForms
{
   using System;
   using System.Drawing;
   using System.Drawing.Drawing2D;
   using System.Runtime.Serialization;

   /// <summary>
   /// GMap.NET marker
   /// </summary>
   [Serializable]
   public class GMapToolTip : ISerializable
   {
      internal GMapMarker Marker;

      public Point Offset;

      /// <summary>
      /// string format
      /// </summary>
      public readonly StringFormat Format = new StringFormat();

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

      #region ISerializable Members

      /// <summary>
      /// Initializes a new instance of the <see cref="GMapToolTip"/> class.
      /// </summary>
      /// <param name="info">The info.</param>
      /// <param name="context">The context.</param>
      protected GMapToolTip(SerializationInfo info, StreamingContext context)
      {
         this.Fill = info.GetValue("Fill", new SolidBrush(Color.FromArgb(222, Color.AliceBlue)));
         this.Font = info.GetValue("Font", new Font(FontFamily.GenericSansSerif, 14, FontStyle.Bold, GraphicsUnit.Pixel));
         this.Format = info.GetValue("Format", new StringFormat());
         this.Offset = info.GetStruct("Offset", Point.Empty);
         this.Stroke = info.GetValue("Stroke", new Pen(Color.FromArgb(140, Color.MidnightBlue)));
         this.TextPadding = info.GetStruct("TextPadding", new Size(10, 10));
      }

      /// <summary>
      /// Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo"/> with the data needed to serialize the target object.
      /// </summary>
      /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> to populate with data.</param>
      /// <param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext"/>) for this serialization.</param>
      /// <exception cref="T:System.Security.SecurityException">
      /// The caller does not have the required permission.
      /// </exception>
      public void GetObjectData(SerializationInfo info, StreamingContext context)
      {
         info.AddValue("Fill", this.Fill);
         info.AddValue("Font", this.Font);
         info.AddValue("Format", this.Format);
         info.AddValue("Offset", this.Offset);
         info.AddValue("Stroke", this.Stroke);
         info.AddValue("TextPadding", this.TextPadding);
      }

      #endregion
   }
}
