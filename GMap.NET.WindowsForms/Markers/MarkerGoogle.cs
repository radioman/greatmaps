using System.Drawing;
using GMapNET.WindowsForms;

namespace GMapNET
{
   public class MarkerGoogle : MapObject
   {
      public bool IsDragging;

      public MarkerGoogle()
      {
         IsDragging = false;
      }

      public void OnRender(Graphics g)
      {
         //if(!IsDragging)
         //{
         //   g.DrawImageUnscaled(Resources.shadow50, LocalPosition.X-10, LocalPosition.Y-34);
         //   g.DrawImageUnscaled(Resources.marker, LocalPosition.X-10, LocalPosition.Y-34);
         //}
         //else
         //{
         //   g.DrawImageUnscaled(Resources.shadow50, LocalPosition.X-10, LocalPosition.Y-40);
         //   g.DrawImageUnscaled(Resources.marker, LocalPosition.X-10, LocalPosition.Y-40);
         //   g.DrawImageUnscaled(Resources.drag_cross_67_16, LocalPosition.X-8, LocalPosition.Y-8);
         //}
      }
   }
}
