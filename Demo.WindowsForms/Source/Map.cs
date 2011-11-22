
namespace Demo.WindowsForms
{
   using System.Windows.Forms;
   using GMap.NET.WindowsForms;
   using System.Drawing;
   using System;

   /// <summary>
   /// custom map of GMapControl
   /// </summary>
   public class Map : GMapControl
   {
      public long ElapsedMilliseconds;

#if DEBUG
      private int counter;
      readonly Font DebugFont = new Font(FontFamily.GenericSansSerif, 14, FontStyle.Regular);
      readonly Font DebugFontSmall = new Font(FontFamily.GenericSansSerif, 12, FontStyle.Bold);

#endif

      DateTime start;
      DateTime end;
      int delta;

      protected override void OnPaint(PaintEventArgs e)
      {
         start = DateTime.Now;

         base.OnPaint(e);

         end = DateTime.Now;
         delta = (int)(end - start).TotalMilliseconds;
      }

      /// <summary>
      /// any custom drawing here
      /// </summary>
      /// <param name="drawingContext"></param>
      protected override void OnPaintOverlays(System.Drawing.Graphics g)
      {
         base.OnPaintOverlays(g);

         //g.ResetTransform();

#if DEBUG
         g.DrawString(Zoom + "z, " + MapProvider + ", refresh: " + counter++ + ", load: " + ElapsedMilliseconds + "ms, render: " + delta + "ms", DebugFont, Brushes.Blue, DebugFont.Height, DebugFont.Height + 20);

         //g.DrawString(ViewAreaPixel.Location.ToString(), DebugFontSmall, Brushes.Blue, DebugFontSmall.Height, DebugFontSmall.Height);

         //string lb = ViewAreaPixel.LeftBottom.ToString();
         //var lbs = g.MeasureString(lb, DebugFontSmall);
         //g.DrawString(lb, DebugFontSmall, Brushes.Blue, DebugFontSmall.Height, Height - DebugFontSmall.Height * 2);

         //string rb = ViewAreaPixel.RightBottom.ToString();
         //var rbs = g.MeasureString(rb, DebugFontSmall);
         //g.DrawString(rb, DebugFontSmall, Brushes.Blue, Width - rbs.Width - DebugFontSmall.Height, Height - DebugFontSmall.Height * 2);

         //string rt = ViewAreaPixel.RightTop.ToString();
         //var rts = g.MeasureString(rb, DebugFontSmall);
         //g.DrawString(rt, DebugFontSmall, Brushes.Blue, Width - rts.Width - DebugFontSmall.Height, DebugFontSmall.Height);
#endif

         g.DrawLine(Pens.Red, Width / 2 - 10, Height / 2, Width / 2 + 10, Height / 2);
         g.DrawLine(Pens.Red, Width / 2, Height / 2 - 10, Width / 2, Height / 2 + 10);
      }
   }
}
