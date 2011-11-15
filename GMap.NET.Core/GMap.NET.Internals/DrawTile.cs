
namespace GMap.NET.Internals
{
   /// <summary>
   /// struct for drawing tile
   /// </summary>
   internal struct DrawTile
   {
      public GPoint PosXY;
      public GPoint PosPixel;

      public DrawTile(GPoint Pos, GPoint PosPixel)
      {
         this.PosXY = Pos;
         this.PosPixel = PosPixel;
      }

      public override string ToString()
      {
         return "PosXY: " + PosXY + ", PosPixel: " + PosPixel;
      }
   }
}
