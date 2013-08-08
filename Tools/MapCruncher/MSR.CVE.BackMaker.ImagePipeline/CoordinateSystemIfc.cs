using System;
using System.Drawing;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public interface CoordinateSystemIfc
	{
		LatLonZoom GetDefaultView();
		LatLonZoom GetTranslationInLatLon(LatLonZoom center, Point offsetInPixels);
		RangeInt GetZoomRange();
		RangeDouble GetLatRange();
		RangeDouble GetLonRange();
		Point GetTranslationInPixels(LatLonZoom center, LatLon location);
		LatLonZoom GetBestViewContaining(MapRectangle newBounds, Size size);
		TileDisplayDescriptorArray GetTileArrayDescriptor(LatLonZoom center, Size windowSize);
		MapRectangle GetUnclippedMapWindow(LatLonZoom latLonZoom, Size size);
		LatLon GetLatLonOfTileNW(TileAddress tileAddress);
		ITileAddressLayout GetTileAddressLayout();
		Size GetTileSize();
	}
}
