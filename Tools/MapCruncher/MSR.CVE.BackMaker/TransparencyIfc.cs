using System;
namespace MSR.CVE.BackMaker
{
	public interface TransparencyIfc
	{
		Pixel GetBaseLayerCenterPixel();
		void InvalidatePipeline();
	}
}
