using System.Collections.Generic;
using System.Drawing;

namespace GMapNET.Internals
{
   /// <summary>
   /// matrix for tiles
   /// </summary>
   /// <typeparam name="T"></typeparam>
   public class TileMatrix
   {
      Dictionary<Point, Tile> matrix;

      public TileMatrix()
      {
         matrix = new Dictionary<Point, Tile>(50);
      }

      public void Clear()
      {
         lock(matrix)
         {
            foreach(Tile t in matrix.Values)
            {
               t.Clear();
            }
            {
               matrix.Clear();
            }
         }
      }

      public void ClearPointsNotIn(ref List<Point> list)
      {
         List<Point> removals = new List<Point>();
         foreach(Point p in matrix.Keys)
         {
            if(!list.Contains(p))
            {
               removals.Add(p);
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
         removals.TrimExcess();
         removals = null;
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
