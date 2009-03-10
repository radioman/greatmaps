
namespace GMapNET
{
   public abstract class MapObject
   {
      public PointLatLng Position;
      public object Tag;

      Point localPosition; 
      public Point LocalPosition
      {
         get
         {
            return localPosition;
         }
         internal set
         {
            localPosition = value;
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
