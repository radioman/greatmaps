using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	internal class ConstantFuture : FutureBase
	{
		private Parameter parameter;
		public ConstantFuture(Parameter parameter)
		{
			this.parameter = parameter;
		}
		public override Present Realize(string refCredit)
		{
			return this.parameter;
		}
		public override void AccumulateRobustHash(IRobustHash hash)
		{
			this.parameter.AccumulateRobustHash(hash);
		}
	}
}
