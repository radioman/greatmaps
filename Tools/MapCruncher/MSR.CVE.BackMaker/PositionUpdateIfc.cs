using System;
namespace MSR.CVE.BackMaker
{
	public interface PositionUpdateIfc
	{
		void PositionUpdated(LatLonZoom llz);
		void ForceInteractiveUpdate();
	}
}
