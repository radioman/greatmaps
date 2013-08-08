using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class AlwaysReturnFailureFuture : IFuture, IRobustlyHashable, IFuturePrototype
	{
		private PresentFailureCode pfc;
		public AlwaysReturnFailureFuture(PresentFailureCode pfc)
		{
			this.pfc = pfc;
		}
		public IFuture Curry(ParamDict paramDict)
		{
			return this;
		}
		public Present Realize(string refCredit)
		{
			return this.pfc;
		}
		public void AccumulateRobustHash(IRobustHash hash)
		{
			hash.Accumulate("DocumentDelayedFuture");
		}
	}
}
