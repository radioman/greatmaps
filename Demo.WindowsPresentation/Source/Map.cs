using System.Windows.Controls;
using System.Windows.Media;

using GMapNET;

namespace Demo.WindowsPresentation
{
   public class Map : GMap
   {
      MarkerDot CurrentMarker = new MarkerDot();

      /// <summary>
      /// any custom drawing here
      /// </summary>
      /// <param name="drawingContext"></param>
      protected override void OnRender(DrawingContext drawingContext)
      {
         base.OnRender(drawingContext);
      }

      /// <summary>
      /// drawing current marker
      /// </summary>
      /// <param name="g"></param>
      protected override void OnDrawCurrentCursor(DrawingContext g)
      {
         if(CurrentMarkerStyle == CurrentMarkerType.Custom)
         {
            CurrentMarker.Position = CurrentPosition;
            CurrentMarker.SetLocalPosition(this);
            CurrentMarker.OnRender(g);
         }
         else
         {
            base.OnDrawCurrentCursor(g);
         }
      }
   }
}
