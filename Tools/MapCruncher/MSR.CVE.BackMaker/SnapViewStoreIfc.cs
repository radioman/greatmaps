using System;
namespace MSR.CVE.BackMaker
{
	public interface SnapViewStoreIfc
	{
		void Record(LatLonZoom llz);
		LatLonZoom Restore();
		void RecordZoom(int zoom);
		int RestoreZoom();
	}
}
