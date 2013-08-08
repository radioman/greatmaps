using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class FlatAddressLayout : ITileAddressLayout
	{
		public int XValueOneTileEast(TileAddress ta)
		{
			return ta.TileX + 1;
		}
		public int YValueOneTileSouth(TileAddress ta)
		{
			return ta.TileY - 1;
		}
	}
}
