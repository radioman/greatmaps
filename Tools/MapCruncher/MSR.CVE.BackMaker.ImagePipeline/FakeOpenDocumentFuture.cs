using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	internal class FakeOpenDocumentFuture : IFuture, IRobustlyHashable, IFuturePrototype
	{
		public Present Realize(string refCredit)
		{
			return new PresentFailureCode(new Exception("No document here."));
		}
		public void AccumulateRobustHash(IRobustHash hash)
		{
			hash.Accumulate("FakeOpenDocumentFuture");
		}
		public IFuture Curry(ParamDict paramDict)
		{
			return this;
		}
	}
}
