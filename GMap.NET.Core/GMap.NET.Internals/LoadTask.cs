
using System;
namespace GMap.NET.Internals
{
   /// <summary>
   /// tile load task
   /// </summary>
   internal struct LoadTask : IEquatable<LoadTask>
   {
      public GPoint Pos;
      public int Zoom;

      public LoadTask(GPoint pos, int zoom)
      {
         Pos = pos;
         Zoom = zoom;
      }

      public override string ToString()
      {
         return Zoom + " - " + Pos.ToString();
      }

      #region IEquatable<DrawTile> Members

      public bool Equals(LoadTask other)
      {
         return (Pos == other.Pos && Zoom == other.Zoom);
      }

      #endregion
   }
}
