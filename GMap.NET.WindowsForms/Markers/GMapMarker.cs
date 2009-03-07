using System.Drawing;

namespace GMapNET
{
   public class GMapMarker : MapObject
   {     
      public MarkerTooltipMode TooltipMode;  
      public Point ToolTipOffset;
      public string ToolTipText;
      public bool Visible;

      private bool isDragging;
      public bool IsDragging
      {
         get
         {
            return isDragging;
         }
         internal set
         {
            isDragging = value;
         }
      }

      private bool isMouseOver; 
      public bool IsMouseOver
      {
         get
         {
            return isMouseOver;
         }
         internal set
         {
            isMouseOver = value;
         }
      }        

      public GMapMarker(PointLatLng pos)
      {
         this.IsDragging = false;
         this.Position = pos;
         this.ToolTipText = string.Empty;
         this.TooltipMode = MarkerTooltipMode.OnMouseOver;
         this.Visible = true;

         this.IsMouseOver = false;
         this.ToolTipOffset = new Point(14, -44);
      }

      public virtual void OnRender(Graphics g)
      {
        //
      }
   }

   public enum MarkerTooltipMode
   {
      OnMouseOver,
      Never,
      Always,
   }
}
