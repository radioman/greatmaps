
namespace GMapNET
{
   public abstract class MapObject
   {
      public PointLatLng Position;
      public object Tag;

      public MapObject()
      {
         Position = PointLatLng.Empty;
      }

      public MapObject(PointLatLng position)
      {
         this.Position = position;
      }
   }
}
