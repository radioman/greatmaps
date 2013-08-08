using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class VEAddressLayout : ITileAddressLayout
	{
		public int XValueOneTileEast(TileAddress ta)
		{
			return VEAddressLayout.WrapLongitude(ta.TileX + 1, ta.ZoomLevel);
		}
		public static int WrapLongitude(int TileX, int ZoomLevel)
		{
			int num = (1 << ZoomLevel) - 1;
			return TileX & num;
		}
		public int YValueOneTileSouth(TileAddress ta)
		{
			return ta.TileY + 1;
		}
	}
}
