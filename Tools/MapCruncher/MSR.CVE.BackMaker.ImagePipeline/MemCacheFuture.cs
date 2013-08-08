using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	internal class MemCacheFuture : FutureBase
	{
		private CacheBase cache;
		private IFuture future;
		public MemCacheFuture(CacheBase cache, IFuture future)
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
			hash.Accumulate("Cache(");
			this.cache.AccumulateRobustHash(hash);
			hash.Accumulate(",");
			this.future.AccumulateRobustHash(hash);
			hash.Accumulate(")");
		}
		internal IFuture GetOpenDocumentFuture()
		{
			if (this.cache is SizeSensitiveCache)
			{
				return this.future;
			}
			return null;
		}
	}
}
