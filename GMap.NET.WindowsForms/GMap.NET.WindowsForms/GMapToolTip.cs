
namespace GMap.NET.WindowsForms
{
   using System;
   using System.Drawing;
   using System.Drawing.Drawing2D;
   using System.Runtime.Serialization;
using System.Collections;
    using System.Collections.Generic;

   /// <summary>
   /// GMap.NET marker
   /// </summary>
   [Serializable]
#if !PocketPC
   public class GMapToolTip : ISerializable, IDisposable
#else
   public class GMapToolTip: IDisposable
#endif
   {
      GMapMarker marker;
      public GMapMarker Marker
      {
         get
         {
            return marker;
         }
         internal set
         {
            marker = value;
         }
      }

      public Point Offset;

      public static readonly StringFormat DefaultFormat = new StringFormat();

      /// <summary>
      /// string format
      /// </summary>
      [NonSerialized]
      public readonly StringFormat Format = DefaultFormat;

#if !PocketPC
      public static readonly Font DefaultFont = new Font(FontFamily.GenericSansSerif, 11, FontStyle.Regular, GraphicsUnit.Pixel);
      public static readonly Font TitleFont = new Font(FontFamily.GenericSansSerif, 11, FontStyle.Bold, GraphicsUnit.Pixel);
#else
      public static readonly Font DefaultFont = new Font(FontFamily.GenericSansSerif, 6, FontStyle.Bold);
#endif

        /// <summary>
        /// font
        /// </summary>
        [NonSerialized]
      public Font Font = DefaultFont;

#if !PocketPC
      public static readonly Pen DefaultStroke = new Pen(Color.FromArgb(140, Color.Black));
#else
      public static readonly Pen DefaultStroke = new Pen(Color.MidnightBlue);
#endif

      /// <summary>
      /// specifies how the outline is painted
      /// </summary>
      [NonSerialized]
      public Pen Stroke = DefaultStroke;

#if !PocketPC
      public static readonly Brush DefaultFill = new SolidBrush(Color.FromArgb(222, Color.White));
#else
      public static readonly Brush DefaultFill = new System.Drawing.SolidBrush(Color.AliceBlue);
#endif

      /// <summary>
      /// background color
      /// </summary>
      [NonSerialized]
      public Brush Fill = DefaultFill;

      public static readonly Brush DefaultForeground = new SolidBrush(Color.DimGray);

      /// <summary>
      /// text foreground
      /// </summary>
      [NonSerialized]
      public Brush Foreground = DefaultForeground;

      /// <summary>
      /// text padding
      /// </summary>
      public Size TextPadding = new Size(20, 21);

      static GMapToolTip()
      {
          DefaultStroke.Width = 1;

#if !PocketPC
          DefaultStroke.LineJoin = LineJoin.Round;
          DefaultStroke.StartCap = LineCap.RoundAnchor;
#endif

#if !PocketPC
          DefaultFormat.LineAlignment = StringAlignment.Near;
#endif
          DefaultFormat.Alignment = StringAlignment.Near;
      }   

      public GMapToolTip(GMapMarker marker)
      {
         this.Marker = marker;
         this.Offset = new Point(14, -44);
      }

      public virtual void OnRender(Graphics g)
      {
         System.Drawing.Size st = g.MeasureString(Marker.ToolTipText, Font).ToSize();
         System.Drawing.RectangleF  rect = new System.Drawing.Rectangle(Marker.ToolTipPosition.X , Marker.ToolTipPosition.Y - st.Height, st.Width + TextPadding.Width, st.Height + TextPadding.Height);
         System.Drawing.RectangleF rectText = new System.Drawing.Rectangle(Marker.ToolTipPosition.X , Marker.ToolTipPosition.Y  - st.Height, st.Width + TextPadding.Width, st.Height + TextPadding.Height);
         rect.Offset(Offset.X, Offset.Y);
         rectText.Offset(Offset.X + 7, Offset.Y + 7);
         g.DrawLine(Stroke, Marker.ToolTipPosition.X, Marker.ToolTipPosition.Y, rect.X, rect.Y + rect.Height / 2);
         g.FillRectangle(Fill, rect);
         //g.DrawRectangle(Stroke, rect);

#if PocketPC
         rect.Offset(0, (rect.Height - st.Height) / 2);
#endif            
         DrawRoundRectangle(g, Stroke, rect.X, rect.Y, rect.Width, rect.Height, 8);
         //g.DrawString(Marker.ToolTipText, Font, Foreground, rectText, Format);
         WriteString(g, Marker.ToolTipText, rectText);
         g.Flush();
      }

#if !PocketPC
      #region ISerializable Members

      /// <summary>
      /// Initializes a new instance of the <see cref="GMapToolTip"/> class.
      /// </summary>
      /// <param name="info">The info.</param>
      /// <param name="context">The context.</param>
      protected GMapToolTip(SerializationInfo info, StreamingContext context)
      {
         this.Offset = Extensions.GetStruct<Point>(info, "Offset", Point.Empty);
         this.TextPadding = Extensions.GetStruct<Size>(info, "TextPadding", new Size(10, 10));
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
         info.AddValue("Offset", this.Offset);
         info.AddValue("TextPadding", this.TextPadding);
      }

      #endregion
#endif

      #region IDisposable Members

      bool disposed = false;

      public void Dispose()
      {
         if(!disposed)
         {
            disposed = true;
         }
      }

        #endregion

        public void DrawRoundRectangle(Graphics g, Pen pen, float h, float v, float width, float height, float radius)
        {
            using (GraphicsPath gp = new GraphicsPath())
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

        private void WriteString(Graphics g, string text, RectangleF rectText)
        {
            string[] vec1 = text.Split('\n');
            for(int i=0; i<vec1.Length; i++)
            {
                if (vec1[i] != "")
                {
                    string[] vec2 = vec1[i].Split('|');
                    if (vec2.Length > 0)
                    {
                        Size st = g.MeasureString(vec2[0], TitleFont).ToSize();
                        g.DrawString(String.Format("{0}", vec2[0]), TitleFont, Foreground, rectText, Format);
                        rectText.X += st.Width + 2;
                        if (vec2.Length > 1)
                        {
                            g.DrawString(vec2[1], Font, Foreground, rectText, Format);
                        }
                        rectText.X -= st.Width + 2;
                        rectText.Y += st.Height;                                             
                    }
                }
                else
                {                   
                    Size st = g.MeasureString("\n", TitleFont).ToSize();
                    rectText.Y += st.Height;
                }                           
            }
        }
    }
}
