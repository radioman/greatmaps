using System.Drawing;
using System.Windows.Forms;
using GMapNET;

namespace Demo.WindowsForms
{
   public class Map : GMap
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
