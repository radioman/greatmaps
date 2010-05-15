
namespace GMap.NET.Internals
{
   using System.Collections.Generic;

   /// <summary>
   /// matrix for tiles
   /// </summary>
   internal class TileMatrix
   {
      readonly List<Dictionary<Point, Tile>> Levels = new List<Dictionary<Point, Tile>>(20);

      public TileMatrix()
      {
         for(int i = 0; i < 22; i++)
         {
            Levels.Add(new Dictionary<Point, Tile>(55));
         }
      }

      public void ClearAllLevels()
      {
         lock(Levels)
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
      }

      public void ClearLevel(int zoom)
      {
         lock(Levels)
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
      }

      readonly List<KeyValuePair<Point, Tile>> tmp = new List<KeyValuePair<Point, Tile>>(44);

      public void ClearLevelAndPointsNotIn(int zoom, List<Point> list)
      {
         lock(Levels)
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
      }

      public void ClearLevelsBelove(int zoom)
      {
         lock(Levels)
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
      }

      public void ClearLevelsAbove(int zoom)
      {
         lock(Levels)
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
      }

      public Tile GetTile(int zoom, Point p)
      {
         lock(Levels)
         {
            Tile ret = null;

            //if(zoom < Levels.Count)
            {
               if(Levels[zoom].TryGetValue(p, out ret))
               {
                  return ret;
               }
            }

            return ret;
         }
      }

      public void SetTile(Tile t)
      {
         lock(Levels)
         {
            if(t.Zoom < Levels.Count)
            {
               Levels[t.Zoom][t.Pos] = t;
            }
         }
      }
   }
}
