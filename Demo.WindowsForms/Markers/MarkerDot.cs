using System.Drawing;
using System.Windows.Forms;
using GMapNET;

namespace Demo.WindowsForms
{
   public class MarkerDot : MapObject
   {
      public Pen Pen;
      public int Radius = 5;

      public MarkerDot()
      {
         Pen = new Pen(Brushes.Red, 2);
      }

      public void OnRender(Graphics g)
      {
         System.Drawing.Point p = new System.Drawing.Point(LocalPosition.X, LocalPosition.Y);
         g.DrawEllipse(Pen, p.X, p.Y, Radius, Radius);
      }
   }
}
