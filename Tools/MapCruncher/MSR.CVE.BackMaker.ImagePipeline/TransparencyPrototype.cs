using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class TransparencyPrototype : IFuturePrototype
	{
		private TransparencyOptions transparencyOptions;
		private IFuturePrototype antialiasedPrototype;
		private IFuturePrototype exactColorPrototype;
		public TransparencyPrototype(TransparencyOptions transparencyOptions, IFuturePrototype antialiasedPrototype, IFuturePrototype exactColorPrototype)
		{
			this.transparencyOptions = new TransparencyOptions(transparencyOptions);
			this.antialiasedPrototype = antialiasedPrototype;
			this.exactColorPrototype = exactColorPrototype;
		}
		public IFuture Curry(ParamDict paramDict)
		{
			return new TransparencyFuture(this.transparencyOptions, this.antialiasedPrototype.Curry(paramDict), this.exactColorPrototype.Curry(paramDict));
		}
	}
}
