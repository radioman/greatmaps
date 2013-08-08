using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	internal class MemCachePrototype : IFuturePrototype
	{
		private CacheBase cache;
		private IFuturePrototype prototype;
		public MemCachePrototype(CacheBase cache, IFuturePrototype prototype)
		{
			this.cache = cache;
			this.prototype = prototype;
		}
		public IFuture Curry(ParamDict paramDict)
		{
			return new MemCacheFuture(this.cache, this.prototype.Curry(paramDict));
		}
	}
}
