using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public interface ITileAddressLayout
	{
		int XValueOneTileEast(TileAddress ta);
		int YValueOneTileSouth(TileAddress ta);
	}
}
