using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class ConstantVerb : Verb
	{
		private Present constantPresent;
		public ConstantVerb()
		{
			this.constantPresent = new PresentFailureCode(new Exception("Null ConstantVerb"));
		}
		public ConstantVerb(Present constantPresent)
		{
			this.constantPresent = constantPresent;
		}
		public Present Evaluate(Present[] paramList)
		{
			return this.constantPresent;
		}
		public void AccumulateRobustHash(IRobustHash hash)
		{
			hash.Accumulate("Constant");
		}
	}
}
