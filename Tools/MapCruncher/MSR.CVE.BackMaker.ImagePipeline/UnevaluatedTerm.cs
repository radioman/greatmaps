using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	internal class UnevaluatedTerm : IFuturePrototype
	{
		private TermName name;
		public UnevaluatedTerm(TermName name)
		{
			this.name = name;
		}
		public IFuture Curry(ParamDict paramDict)
		{
			return new ConstantFuture(paramDict[this.name]);
		}
	}
}
