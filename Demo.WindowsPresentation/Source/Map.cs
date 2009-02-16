using System.Windows.Controls;
using System.Windows.Media;
using GMapNET;

namespace Demo.WindowsPresentation
{
   public class Map : GMap
   {
      public Map()
      {
         // ...
      }

      /// <summary>
      /// any custom drawing here
      /// </summary>
      /// <param name="drawingContext"></param>
      protected override void OnRender(DrawingContext drawingContext)
      {
         base.OnRender(drawingContext);
      }
   }
}
