using System;
using System.Collections.Generic;

namespace GMapNET.Internals
{
   internal class Tile
   {
      RenderMode mode;
      Point pos;
      int zoom;
      public readonly List<PureImage> Overlays = new List<PureImage>(1);

      public Tile(int zoom, Point pos, RenderMode mode)
      {
         this.mode = mode;
         this.Zoom = zoom;
         this.Pos = pos;
      }

      public void Clear()
      {
         lock(Overlays)
         {
            foreach(PureImage img in Overlays)
            {
               if(img != null)
               {
                  img.Dispose();
               }
            }
            Overlays.Clear();
            Overlays.TrimExcess();
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

      public Point Pos
      {
         get
         {
            return pos;
         }
         private set
         {
            pos= value;
         }
      }
   }
}
