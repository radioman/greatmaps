using System;
namespace MSR.CVE.BackMaker
{
	internal class SourceSnapView : SnapViewStoreIfc
	{
		private SourceMapViewManager smvm;
		public SourceSnapView(SourceMapViewManager smvm)
		{
			this.smvm = smvm;
		}
		public void Record(LatLonZoom llz)
		{
			this.smvm.RecordSource(llz);
		}
		public LatLonZoom Restore()
		{
			return this.smvm.RestoreSource();
		}
		public void RecordZoom(int zoom)
		{
			this.smvm.RecordSourceZoom(zoom);
		}
		public int RestoreZoom()
		{
			return this.smvm.RestoreSourceZoom();
		}
	}
}
