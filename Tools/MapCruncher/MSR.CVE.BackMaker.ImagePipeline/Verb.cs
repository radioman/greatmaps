using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public interface Verb
	{
		Present Evaluate(Present[] paramList);
		void AccumulateRobustHash(IRobustHash hash);
	}
}
