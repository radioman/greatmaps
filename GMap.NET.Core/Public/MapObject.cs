
namespace GMapNET
{
   /// <summary>
   /// represents object on the map
   /// </summary>
   public abstract class MapObject
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

      public MapObject()
      {
         Position = PointLatLng.Empty;
         LocalPosition = Point.Empty;
      }

      public MapObject(PointLatLng position)
      {
         Position = position;
         LocalPosition = Point.Empty;
      }
   }
}
