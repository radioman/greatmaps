namespace GMap.NET.Internals
{
   using System.IO;
   using System;

   /// <summary>
   /// struct for raw tile
   /// </summary>
   internal struct RawTile
   {
      public int Type;
      public GPoint Pos;
      public int Zoom;

      public RawTile(int Type, GPoint Pos, int Zoom)
      {
         this.Type = Type;
         this.Pos = Pos;
         this.Zoom = Zoom;
      }

      public override string ToString()
      {
         return Type + " at zoom " + Zoom + ", pos: " + Pos;
      }
   }
}
