using System.Drawing;
using System.Windows.Forms;
using GMapNET;

namespace Demo.WindowsForms
{
   public class Map : GMap
   {
      MarkerDot CurrentMarker = new MarkerDot();

      /// <summary>
      /// any custom drawing here
      /// </summary>
      /// <param name="drawingContext"></param>
      protected override void OnPaint(PaintEventArgs e)
      {
         base.OnPaint(e);
      }

      /// <summary>
      /// drawing current marker
      /// </summary>
      /// <param name="g"></param>
      protected override void OnDrawCurrentMarker(Graphics g)
      {
         if(CurrentMarkerStyle == CurrentMarkerType.Custom)
         {
            //CurrentMarker.Position = CurrentPosition;
            //CurrentMarker.SetLocalPosition(this);
            //CurrentMarker.OnRender(g);
         }
         else
         {
            base.OnDrawCurrentMarker(g);
         }
      }
   }
}
