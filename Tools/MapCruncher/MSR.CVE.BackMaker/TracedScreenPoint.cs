using System;
using System.Drawing;
namespace MSR.CVE.BackMaker
{
	public class TracedScreenPoint
	{
		public int originalIndex;
		public PointF pointf;
		public TracedScreenPoint(int originalIndex, PointF pointf)
		{
			this.originalIndex = originalIndex;
			this.pointf = pointf;
		}
	}
}
