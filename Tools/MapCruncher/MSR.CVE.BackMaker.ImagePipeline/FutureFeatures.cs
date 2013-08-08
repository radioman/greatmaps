using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public enum FutureFeatures
	{
		Raw,
		MemoryCached,
		DiskCached,
		Cached,
		Async,
		Transparency = 8,
		ExactColors = 16
	}
}
