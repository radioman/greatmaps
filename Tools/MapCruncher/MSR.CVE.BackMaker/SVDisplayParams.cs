using System;
using System.Drawing;
namespace MSR.CVE.BackMaker
{
	public interface SVDisplayParams : InvalidatableViewIfc
	{
		LatLonZoom MapCenter();
		Point ScreenCenter();
	}
}
