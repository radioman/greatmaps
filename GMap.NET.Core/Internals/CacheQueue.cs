
using System.IO;

namespace GMapNET.Internals
{
   /// <summary>
   /// cache queue item
   /// </summary>
   internal struct CacheQueue
   {
      public MapType Type;
      public Point Pos;
      public int Zoom;
      public MemoryStream Img;

      public CacheQueue(MapType Type, Point Pos, int Zoom, MemoryStream Img)
      {
         this.Type = Type;
         this.Pos = Pos;
         this.Zoom = Zoom;
         this.Img = Img;
      }
   }
}
