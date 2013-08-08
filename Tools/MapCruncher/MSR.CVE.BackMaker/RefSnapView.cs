using System;
namespace MSR.CVE.BackMaker
{
	internal class RefSnapView : SnapViewStoreIfc
	{
		private SourceMapViewManager smvm;
		public RefSnapView(SourceMapViewManager smvm)
		{
			this.smvm = smvm;
		}
		public void Record(LatLonZoom llz)
		{
			this.smvm.RecordRef(llz);
		}
		public LatLonZoom Restore()
		{
			return this.smvm.RestoreRef();
		}
		public void RecordZoom(int zoom)
		{
			this.smvm.RecordRefZoom(zoom);
		}
		public int RestoreZoom()
		{
			return this.smvm.RestoreRefZoom();
		}
	}
}
