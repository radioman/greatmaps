using MSR.CVE.BackMaker.ImagePipeline.AsynchronizerPrivate;
using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	internal class Asynchronizer : IFuturePrototype
	{
		private AsyncScheduler scheduler;
		private IFuturePrototype innerPrototype;
		public Asynchronizer(AsyncScheduler scheduler, IFuturePrototype innerPrototype)
		{
			this.scheduler = scheduler;
			this.innerPrototype = innerPrototype;
		}
		public IFuture Curry(ParamDict paramDict)
		{
			return Asynchronizer.MakeFuture(this.scheduler, this.innerPrototype.Curry(paramDict));
		}
		public static IFuture MakeFuture(AsyncScheduler scheduler, IFuture innerFuture)
		{
			return new MemCacheFuture(scheduler.GetCache(), new AsynchronizerFuture(scheduler, innerFuture));
		}
	}
}
