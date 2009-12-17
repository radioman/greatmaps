
namespace GMap.NET.Internals
{
   using System.Collections.Generic;

   /// <summary>
   /// matrix for tiles
   /// </summary>
   internal class TileMatrix
   {
      readonly Dictionary<Point, Tile> matrix = new Dictionary<Point, Tile>(55);
      readonly List<Point> removals = new List<Point>();

      public void Clear()
      {
         lock(matrix)
         {
            foreach(Tile t in matrix.Values)
            {
               t.Clear();
            }
            matrix.Clear();
         }
      }       

      public void ClearPointsNotIn(ref List<Point> list)
      {
         removals.Clear();
         lock(matrix)
         {
            foreach(Point p in matrix.Keys)
            {
               if(!list.Contains(p))
               {
                  removals.Add(p);
               }
            }
         }

         foreach(Point p in removals)
         {
            Tile t = this[p];
            if(t != null)
            {
               lock(matrix)
               {
                  t.Clear();
                  t = null;

                  matrix.Remove(p);
               }
            }
         }
         removals.Clear();
      }

      public Tile this[Point p]
      {
         get
         {
            lock(matrix)
            {
               Tile ret = null;
               if(matrix.TryGetValue(p, out ret))
               {
                  return ret;
               }
               else
               {
                  return null;
               }
            }
         }

         set
         {
            lock(matrix)
            {
               matrix[p] = value;
            }
         }
      }
   }
}
