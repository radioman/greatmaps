
namespace GMap.NET.Internals
{
   using System.Collections.Generic;
   using System.IO;

   /// <summary>
   /// kiber speed memory cache for tiles with history support ;}
   /// </summary>
   internal class KiberTileCache : Dictionary<RawTile, MemoryStream>
   {
      Queue<RawTile> Queue = new Queue<RawTile>();

      /// <summary>
      /// the amount of tiles in MB to keep in memmory, default: 22MB, if each ~100Kb it's ~222 tiles
      /// </summary>
      public int MemoryCacheCapacity = 22;

      long memoryCacheSize = 0;

      /// <summary>
      /// current memmory cache size in MB
      /// </summary>
      public double MemoryCacheSize
      {
         get
         {
            return memoryCacheSize/1048576.0;
         }
      }

      public new void Add(RawTile key, MemoryStream value)
      {
         // clear oldest values
         if(MemoryCacheSize > MemoryCacheCapacity)
         {
            RemoveOldest();
         }

         Queue.Enqueue(key);
         base.Add(key, value);

         memoryCacheSize += value.Length;
      }

      // do not allow directly removal of elements
      private new void Remove(RawTile key)
      {

      }

      private bool RemoveOldest()
      {
         if(Keys.Count > 0)
         {
            RawTile first = Queue.Dequeue();
            MemoryStream m = base[first];
            base.Remove(first);
            memoryCacheSize -= m.Length;
            m.Dispose();
            return true;
         }
         return false;
      }
   }
}
