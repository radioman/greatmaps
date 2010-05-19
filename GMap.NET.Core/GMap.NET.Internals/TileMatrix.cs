
namespace GMap.NET.Internals
{
   using System.Collections.Generic;
   using System.Threading;

   /// <summary>
   /// matrix for tiles
   /// </summary>
   internal class TileMatrix
   {
      readonly List<Dictionary<Point, Tile>> Levels = new List<Dictionary<Point, Tile>>(20);
      readonly ReaderWriterLock Lock = new ReaderWriterLock();

      public TileMatrix()
      {
         for(int i = 0; i < 22; i++)
         {
            Levels.Add(new Dictionary<Point, Tile>(55));
         }
      }

      public void ClearAllLevels()
      {
         Lock.AcquireWriterLock(-1);
         try
         {
            foreach(var matrix in Levels)
            {
               foreach(var t in matrix)
               {
                  t.Value.Clear();
               }
               matrix.Clear();
            }
         }
         finally
         {
            Lock.ReleaseWriterLock();
         }
      }

      public void ClearLevel(int zoom)
      {
         Lock.AcquireWriterLock(-1);
         try
         {
            if(zoom < Levels.Count)
            {
               var l = Levels[zoom];

               foreach(var t in l)
               {
                  t.Value.Clear();
               }

               l.Clear();
            }
         }
         finally
         {
            Lock.ReleaseWriterLock();
         }
      }

      readonly List<KeyValuePair<Point, Tile>> tmp = new List<KeyValuePair<Point, Tile>>(44);

      public void ClearLevelAndPointsNotIn(int zoom, List<Point> list)
      {
         Lock.AcquireWriterLock(-1);
         try
         {
            if(zoom < Levels.Count)
            {
               var l = Levels[zoom];

               tmp.Clear();

               foreach(var t in l)
               {
                  if(!list.Contains(t.Key))
                  {
                     tmp.Add(t);
                  }
               }

               foreach(var r in tmp)
               {
                  l.Remove(r.Key);
                  r.Value.Clear();
               }

               tmp.Clear();
            }
         }
         finally
         {
            Lock.ReleaseWriterLock();
         }
      }

      public void ClearLevelsBelove(int zoom)
      {
         Lock.AcquireWriterLock(-1);
         try
         {
            if(zoom-1 < Levels.Count)
            {
               for(int i = zoom-1; i >= 0; i--)
               {
                  var l = Levels[i];

                  foreach(var t in l)
                  {
                     t.Value.Clear();
                  }

                  l.Clear();
               }
            }
         }
         finally
         {
            Lock.ReleaseWriterLock();
         }
      }

      public void ClearLevelsAbove(int zoom)
      {
         Lock.AcquireWriterLock(-1);
         try
         {
            if(zoom+1 < Levels.Count)
            {
               for(int i = zoom+1; i < Levels.Count; i++)
               {
                  var l = Levels[i];

                  foreach(var t in l)
                  {
                     t.Value.Clear();
                  }

                  l.Clear();
               }
            }
         }
         finally
         {
            Lock.ReleaseWriterLock();
         }
      }

      public Tile GetTile(int zoom, Point p)
      {
         Tile ret = null;

         Lock.AcquireReaderLock(-1);
         try
         {
            //if(zoom < Levels.Count)
            {
               Levels[zoom].TryGetValue(p, out ret);
            }
         }
         finally
         {
            Lock.ReleaseReaderLock();
         }

         return ret;
      }

      public void SetTile(Tile t)
      {
         Lock.AcquireWriterLock(-1);
         try
         {
            if(t.Zoom < Levels.Count)
            {
               Levels[t.Zoom][t.Pos] = t;
            }
         }
         finally
         {
            Lock.ReleaseWriterLock();
         }
      }
   }
}
