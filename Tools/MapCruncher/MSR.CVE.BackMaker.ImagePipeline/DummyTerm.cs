using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	internal class DummyTerm : Parameter, IRobustlyHashable, Present, IDisposable
	{
		public void AccumulateRobustHash(IRobustHash hash)
		{
			hash.Accumulate("DummyTerm");
		}
		public Present Duplicate(string refCredit)
		{
			return this;
		}
		public void Dispose()
		{
		}
	}
}
