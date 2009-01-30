
namespace GMapNET
{
   public abstract class MapObject
   {
      public PointLatLng Position;
      public int Width;
      public int Height;

      public MapObject()
      {
         Position = PointLatLng.Empty;
         Width = 5;
         Height = 5;
      }

      public MapObject(PointLatLng position)
      {
         this.Position = position;
      }
   }
}
