using System;
namespace MSR.CVE.BackMaker.ImagePipeline.AsynchronizerPrivate
{
	internal class AsynchronizerFuture : FutureBase
	{
		private AsyncScheduler scheduler;
		private IFuture innerFuture;
		public AsynchronizerFuture(AsyncScheduler scheduler, IFuture innerFuture)
		{
			this.scheduler = scheduler;
			this.innerFuture = innerFuture;
		}
		public override Present Realize(string refCredit)
		{
			return new AsyncRef(new AsyncRecord(this.scheduler, this, this.innerFuture), refCredit);
		}
		public override void AccumulateRobustHash(IRobustHash hash)
		{
			hash.Accumulate("AsynchronizerFuture(");
			this.innerFuture.AccumulateRobustHash(hash);
			hash.Accumulate(")");
		}
	}
}
