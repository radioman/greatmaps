using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class BoolParameter : ImmutableParameter<bool>
	{
		public BoolParameter(bool value) : base(value)
		{
		}
		public override void AccumulateRobustHash(IRobustHash hash)
		{
			hash.Accumulate(base.value);
		}
	}
}
