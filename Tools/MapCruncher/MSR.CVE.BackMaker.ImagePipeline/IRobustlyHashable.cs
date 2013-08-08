using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public interface IRobustlyHashable
	{
		void AccumulateRobustHash(IRobustHash hash);
	}
}
