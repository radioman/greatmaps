
namespace GMap.NET.Internals
{
   using System.IO;

   /// <summary>
   /// cache queue item
   /// </summary>
   internal struct CacheQueue
   {
      public MapType Type;
      public Point Pos;
      public int Zoom;
      public MemoryStream Img;
      public CacheUsage CacheType;

      public CacheQueue(MapType Type, Point Pos, int Zoom, MemoryStream Img, CacheUsage cacheType)
      {
         this.Type = Type;
         this.Pos = Pos;
         this.Zoom = Zoom;
         this.Img = Img;
         this.CacheType = cacheType;
      }
   }

   internal enum CacheUsage
   {
      First = 0,
      Second = 1,
      Both = First | Second
   }
}
