
namespace Demo.WindowsPresentation
{
   using System.Windows.Controls;
   using System.Windows.Media;
   using GMap.NET.WindowsPresentation;

   /// <summary>
   /// the custom map f GMapControl 
   /// </summary>
   public class Map : GMapControl
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
