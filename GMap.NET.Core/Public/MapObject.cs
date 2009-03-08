
namespace GMapNET
{
   public abstract class MapObject
   {
      public PointLatLng Position;
      public object Tag;

      internal Point LocalPosition;

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
