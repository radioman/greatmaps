namespace GMap.NET.Internals
{
   using System.IO;

   /// <summary>
   /// struct for raw tile
   /// </summary>
   internal struct RawTile
   {
      public MapType Type;
      public Point Pos;
      public int Zoom;

      public RawTile(MapType Type, Point Pos, int Zoom)
      {
         this.Type = Type;
         this.Pos = Pos;
         this.Zoom = Zoom;
      }
   }
}
