
namespace Demo.WindowsForms
{
   using System.Windows.Forms;
   using GMap.NET.WindowsForms;

   /// <summary>
   /// custom map of GMapControl
   /// </summary>
   public class Map : GMapControl
   {
      /// <summary>
      /// any custom drawing here
      /// </summary>
      /// <param name="drawingContext"></param>
      protected override void OnPaint(PaintEventArgs e)
      {
         base.OnPaint(e);            
      }
   }
}
