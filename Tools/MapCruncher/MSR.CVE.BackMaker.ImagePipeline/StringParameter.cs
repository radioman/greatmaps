using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class StringParameter : ImmutableParameter<string>
	{
		public StringParameter(string value) : base(value)
		{
		}
		public override void AccumulateRobustHash(IRobustHash hash)
		{
			hash.Accumulate(base.value);
		}
	}
}
