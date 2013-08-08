using System;
namespace MSR.CVE.BackMaker
{
	public class DefaultReferenceView
	{
		public bool present;
		public LatLonZoom llz;
		public DefaultReferenceView(LatLonZoom llz)
		{
			this.present = true;
			this.llz = llz;
		}
		public DefaultReferenceView()
		{
		}
	}
}
