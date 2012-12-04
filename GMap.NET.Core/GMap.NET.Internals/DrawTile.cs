
namespace GMap.NET.Internals
{
   /// <summary>
   /// struct for drawing tile
   /// </summary>
   internal struct DrawTile
   {
      public GPoint PosXY { get; set; }
      public GPoint PosPixel { get; set; }
      public double DistanceSqr { get; set; }

      public override string ToString()
      {
         return PosXY + ", px: " + PosPixel;
      }
   }
}
