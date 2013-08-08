using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class TileAddressToImageRegion : Verb
	{
		private CoordinateSystemIfc coordinateSystem;
		public TileAddressToImageRegion(CoordinateSystemIfc coordinateSystem)
		{
			this.coordinateSystem = coordinateSystem;
		}
		public Present Evaluate(Present[] paramList)
		{
			D.Assert(paramList.Length == 1);
			TileAddress tileAddress = (TileAddress)paramList[0];
			if (tileAddress.ZoomLevel < 1)
			{
				return new PresentFailureCode(new Exception("zoomlevel 0"));
			}
			ITileAddressLayout tileAddressLayout = this.coordinateSystem.GetTileAddressLayout();
			TileAddress tileAddress2 = new TileAddress(tileAddressLayout.XValueOneTileEast(tileAddress), tileAddressLayout.YValueOneTileSouth(tileAddress), tileAddress.ZoomLevel);
			LatLon latLonOfTileNW = this.coordinateSystem.GetLatLonOfTileNW(tileAddress);
			LatLon latLonOfTileNW2 = this.coordinateSystem.GetLatLonOfTileNW(tileAddress2);
			if (latLonOfTileNW2.lon <= latLonOfTileNW.lon)
			{
				latLonOfTileNW2 = new LatLon(latLonOfTileNW2.lat, latLonOfTileNW2.lon + 360.0);
			}
			D.Assert(latLonOfTileNW2.lon > latLonOfTileNW.lon);
			return new MapRectangleParameter(new MapRectangle(latLonOfTileNW, latLonOfTileNW2));
		}
		public void AccumulateRobustHash(IRobustHash hash)
		{
			hash.Accumulate("TileAddressToImageRegion");
		}
	}
}
