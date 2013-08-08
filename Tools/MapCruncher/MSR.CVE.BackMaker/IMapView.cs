using System;
namespace MSR.CVE.BackMaker
{
	public interface IMapView : ICurrentView
	{
		MapPosition GetReferenceMapView();
		LatLonZoom GetSourceMapView();
	}
}
