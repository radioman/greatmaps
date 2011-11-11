
namespace GMap.NET.Internals
{
   using System.Collections.Generic;
   using System;

   /// <summary>
   /// represent tile
   /// </summary>
   public struct Tile : IDisposable
   {
      public static readonly Tile Empty = new Tile();

      GPoint pos;
      int zoom;
      public List<PureImage> Overlays;

      public Tile(int zoom, GPoint pos)
      {
         this.zoom = zoom;
         this.pos = pos;
         this.Overlays = new List<PureImage>();
      }

      void Clear()
      {
         if(Overlays != null)
         {
            lock(Overlays)
            {
               foreach(PureImage i in Overlays)
               {
                  i.Dispose();
               }

               Overlays.Clear();
            }
            Overlays = null;
         }
      }

      public int Zoom
      {
         get
         {
            return zoom;
         }
         private set
         {
            zoom = value;
         }
      }

      public GPoint Pos
      {
         get
         {
            return pos;
         }
         private set
         {
            pos = value;
         }
      }

      #region IDisposable Members

      public void Dispose()
      {
         Clear();
      }

      #endregion

      public static bool operator ==(Tile m1, Tile m2)
      {
         return m1.pos == m2.pos && m1.zoom == m2.zoom;
      }

      public static bool operator !=(Tile m1, Tile m2)
      {
         return !(m1 == m2);
      }

      public override bool Equals(object obj)
      {
         return base.Equals(obj);
      }

      public override int GetHashCode()
      {
         return base.GetHashCode();
      }
   }
}
