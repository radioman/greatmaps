using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class IntParameter : ImmutableParameter<int>
	{
		public IntParameter(int value) : base(value)
		{
		}
		public override void AccumulateRobustHash(IRobustHash hash)
		{
			hash.Accumulate(base.value);
		}
	}
}
