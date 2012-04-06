
namespace GMap.NET.Internals
{
   using System;
   using System.Collections.Generic;
   using System.Diagnostics;
   using System.Threading;

   /// <summary>
   /// represent tile
   /// </summary>
   public struct Tile : IDisposable
   {
      public static readonly Tile Empty = new Tile();

      GPoint pos;
      int zoom;
      PureImage[] overlays;
      long OverlaysCount;

      public readonly bool NotEmpty;

      public Tile(int zoom, GPoint pos)
      {
         this.NotEmpty = true;
         this.zoom = zoom;
         this.pos = pos;
         this.overlays = null;
         this.OverlaysCount = 0;
      }

      public IEnumerable<PureImage> Overlays
      {
         get
         {
            for(long i = 0, size = Interlocked.Read(ref OverlaysCount); i < size; i++)
            {
               yield return overlays[i];
            }
         }
      }

      internal void AddOverlay(PureImage i)
      {
         if(overlays == null)
         {
            overlays = new PureImage[4];
         }
         overlays[Interlocked.Increment(ref OverlaysCount) - 1] = i;
      }

      internal bool HasAnyOverlays
      {
         get
         {
            return Interlocked.Read(ref OverlaysCount) > 0;
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
         if(overlays != null)
         {
            for(long i = Interlocked.Read(ref OverlaysCount) - 1; i >= 0; i--)
            {
               Interlocked.Decrement(ref OverlaysCount);

               overlays[i].Dispose();
               overlays[i] = null;
            }
            overlays = null;
         }
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
