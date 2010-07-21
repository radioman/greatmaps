
namespace Demo.WindowsForms
{
   using System.Windows.Forms;
   using GMap.NET.WindowsForms;
   using System.Drawing;

   /// <summary>
   /// custom map of GMapControl
   /// </summary>
   public class Map : GMapControl
   {
      public long ElapsedMilliseconds;

#if DEBUG
      private int counter;
      readonly Font DebugFont = new Font(FontFamily.GenericSansSerif, 36, FontStyle.Regular);
#endif

      /// <summary>
      /// any custom drawing here
      /// </summary>
      /// <param name="drawingContext"></param>
      protected override void OnPaintEtc(System.Drawing.Graphics g)
      {
         base.OnPaintEtc(g);

#if DEBUG
         g.DrawString("render: " + counter++ + ", load: " + ElapsedMilliseconds + "ms", DebugFont, Brushes.Blue, 36, 36);
#endif
      }
   }
}
