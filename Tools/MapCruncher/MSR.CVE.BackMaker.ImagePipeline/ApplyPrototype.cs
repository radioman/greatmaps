using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class ApplyPrototype : IFuturePrototype
	{
		private Verb verb;
		private IFuturePrototype[] prototypeParams;
		public ApplyPrototype(Verb verb, params IFuturePrototype[] prototypeParams)
		{
			this.verb = verb;
			this.prototypeParams = prototypeParams;
		}
		public IFuture Curry(ParamDict paramDict)
		{
			IFuture[] futureParams = Array.ConvertAll<IFuturePrototype, IFuture>(this.prototypeParams, (IFuturePrototype p) => p.Curry(paramDict));
			return new ApplyFuture(this.verb, futureParams);
		}
	}
}
