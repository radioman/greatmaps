using System.Drawing;

namespace GMapNET
{
   public class Marker
   {
      public MarkerType Type;
      public PointLatLng Position;
      public MarkerColor Color;
      public MarkerTooltipMode TooltipMode;  
      public Point ToolTipOffset;
      public CustomMarkerAlign CustomMarkerAlign;
      public Point CustomMarkerCenter;
      public string Text;
      public bool Visible;
      public Image CustomMarker;
      public object Tag;

      private bool isMouseOver; 
      public bool IsMouseOver
      {
         get
         {
            return isMouseOver;
         }
         set
         {
            isMouseOver = value;
         }
      }        

      public Marker(PointLatLng pos, MarkerType type, MarkerColor color)
      {
         this.Position = pos;
         this.Type = type;
         this.Color = color;
         this.Text = string.Empty;
         this.TooltipMode = MarkerTooltipMode.OnMouseOver;
         this.Visible = true;
         this.CustomMarkerAlign = CustomMarkerAlign.MiddleMiddle;
         this.CustomMarkerCenter = Point.Empty;

         this.IsMouseOver = false;
         this.ToolTipOffset = new Point(14, -44);
      }
   }

   public enum MarkerType
   {
      Small,
      Medium,
      Custom,
   }

   public enum CustomMarkerAlign
   {
      MiddleMiddle,
      Manual,
   }

   public enum CurrentMarkerType
   {
      GMap,
      Cross,
      Custom,
   }

   public enum MarkerColor
   {
      Red,
      Green,
      Yellow,
      Blue,
   }

   public enum MarkerTooltipMode
   {
      OnMouseOver,
      Never,
      Always,
   }
}
