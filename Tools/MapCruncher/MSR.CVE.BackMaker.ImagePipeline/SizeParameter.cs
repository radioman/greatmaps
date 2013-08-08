using System;
using System.Drawing;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class SizeParameter : ImmutableParameter<Size>
	{
		public SizeParameter(Size value) : base(value)
		{
		}
		public override void AccumulateRobustHash(IRobustHash hash)
		{
			hash.Accumulate(base.value);
		}
	}
}
