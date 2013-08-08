using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public interface IEvictable
	{
		bool EvictMeNow();
	}
}
