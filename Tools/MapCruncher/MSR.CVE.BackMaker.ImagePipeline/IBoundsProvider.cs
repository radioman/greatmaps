using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public interface IBoundsProvider
	{
		RenderRegion GetRenderRegion();
	}
}
