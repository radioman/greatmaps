using System;
using System.Drawing;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class CoordinateSystemUtilities
	{
		public static double DegreesToRadians(double degrees)
		{
			return degrees * 3.1415926535897931 / 180.0;
		}
		public static double RadiansToDegrees(double radians)
		{
			return radians * 180.0 / 3.1415926535897931;
		}
		public static LatLonZoom GetZoomedView(CoordinateSystemIfc coordinateSystem, LatLonZoom oldCenterPosition, int steps)
		{
			int num = oldCenterPosition.zoom + steps;
			num = coordinateSystem.GetZoomRange().Constrain(num);
			return new LatLonZoom(oldCenterPosition.lat, oldCenterPosition.lon, num);
		}
		public static MapRectangle GetBounds(CoordinateSystemIfc coordinateSystem, LatLonZoom center, Size size)
		{
			LatLon latlon = coordinateSystem.GetTranslationInLatLon(center, new Point(size.Width / 2, size.Height / 2)).latlon;
			LatLon latlon2 = coordinateSystem.GetTranslationInLatLon(center, new Point(-size.Width / 2, -size.Height / 2)).latlon;
			return new MapRectangle(latlon, latlon2);
		}
		public static MapRectangle GetRangeAsMapRectangle(CoordinateSystemIfc coordSys)
		{
			return new MapRectangle(coordSys.GetLatRange().min, coordSys.GetLonRange().min, coordSys.GetLatRange().max, coordSys.GetLonRange().max);
		}
		public static MapRectangle TileAddressToMapRectangle(CoordinateSystemIfc coordinateSystem, TileAddress tileAddress)
		{
			TileAddress tileAddress2 = new TileAddress(coordinateSystem.GetTileAddressLayout().XValueOneTileEast(tileAddress), coordinateSystem.GetTileAddressLayout().YValueOneTileSouth(tileAddress), tileAddress.ZoomLevel);
			LatLon latLonOfTileNW = coordinateSystem.GetLatLonOfTileNW(tileAddress);
			LatLon latLonOfTileNW2 = coordinateSystem.GetLatLonOfTileNW(tileAddress2);
			if (tileAddress2.TileX < tileAddress.TileX)
			{
				latLonOfTileNW2 = new LatLon(latLonOfTileNW2.lat, latLonOfTileNW2.lon + 360.0);
			}
			if (tileAddress2.TileY < tileAddress.TileY)
			{
				D.Assert(false, "study this case");
				latLonOfTileNW2 = new LatLon(latLonOfTileNW2.lat - 180.0, latLonOfTileNW2.lon);
			}
			return new MapRectangle(latLonOfTileNW, latLonOfTileNW2);
		}
		internal static LatLonZoom ConstrainLLZ(CoordinateSystemIfc coordSys, LatLonZoom src)
		{
			return new LatLonZoom(coordSys.GetLatRange().Constrain(src.lat), coordSys.GetLonRange().Constrain(src.lon), coordSys.GetZoomRange().Constrain(src.zoom));
		}
	}
}
