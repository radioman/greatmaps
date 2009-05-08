using System.Drawing;

namespace GMapNET
{
   /// <summary>
   /// GMap.NET marker
   /// </summary>
   public class GMapMarker
   {
      public PointLatLng Position;
      public object Tag;

      Rectangle area;
      public Point LocalPosition
      {
         get
         {
            return area.Location;
         }
         internal set
         {
            area.Location = value;
         }
      }

      public Size Size
      {
         get
         {
            return area.Size;
         }
         set
         {
            area.Size = value;
         }
      }

      internal Rectangle LocalArea
      {
         get
         {
            Rectangle ret = area;
            ret.Offset(-Size.Width/2, -Size.Height/2);
            return ret;
         }
      }

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
         this.Position = pos;
         this.IsDragging = false;
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

   public delegate void MarkerClick(GMapMarker item);
   public delegate void MarkerEnter(GMapMarker item);
   public delegate void MarkerLeave(GMapMarker item);

   public enum MarkerTooltipMode
   {
      OnMouseOver,
      Never,
      Always,
   }
}
