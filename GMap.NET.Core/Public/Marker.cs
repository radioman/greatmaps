
namespace GMapNET
{
   public class Marker : MapObject
   {     
      public MarkerTooltipMode TooltipMode;  
      public Point ToolTipOffset;
      public string Text;
      public bool Visible;
      public object Tag;

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

      public Marker(PointLatLng pos)
      {
         this.Position = pos;
         this.Text = string.Empty;
         this.TooltipMode = MarkerTooltipMode.OnMouseOver;
         this.Visible = true;

         this.IsMouseOver = false;
         this.ToolTipOffset = new Point(14, -44);
      }
   }

   public enum CurrentMarkerType
   {
      GMap,
      Cross,
      Custom,
   }

   public enum MarkerTooltipMode
   {
      OnMouseOver,
      Never,
      Always,
   }
}
