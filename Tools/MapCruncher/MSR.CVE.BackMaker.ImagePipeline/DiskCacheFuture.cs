using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	internal class DiskCacheFuture : FutureBase
	{
		private DiskCache cache;
		private IFuture future;
		public DiskCacheFuture(DiskCache cache, IFuture future)
		{
			this.cache = cache;
			this.future = future;
		}
		public override Present Realize(string refCredit)
		{
			return this.cache.Get(this.future, refCredit);
		}
		public override void AccumulateRobustHash(IRobustHash hash)
		{
			hash.Accumulate("DiskCache(");
			this.future.AccumulateRobustHash(hash);
			hash.Accumulate(")");
		}
	}
}
