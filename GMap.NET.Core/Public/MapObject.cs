
namespace GMapNET
{
   public abstract class MapObject
   {
      public PointLatLng Position;
      private Point localPosition;      
      public int Width;
      public int Height;

      public Point LocalPosition
      {
         get
         {
            return localPosition;
         }
         private set
         {
            localPosition = value;
         }
      }

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

      /// <summary>
      /// sets local position for rendering
      /// </summary>
      /// <param name="map"></param>
      public void SetLocalPosition(IGControl map)
      {
         LocalPosition = map.FromLatLngToLocal(Position);
      }
   }
}
