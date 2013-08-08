using System;
namespace MSR.CVE.BackMaker
{
	public class TracedVertex
	{
		public int originalIndex;
		public LatLon position;
		public TracedVertex(int originalIndex, LatLon position)
		{
			this.originalIndex = originalIndex;
			this.position = position;
		}
	}
}
