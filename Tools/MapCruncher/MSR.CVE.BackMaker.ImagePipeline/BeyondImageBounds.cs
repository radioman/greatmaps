using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class BeyondImageBounds : Present, IDisposable
	{
		public Present Duplicate(string refCredit)
		{
			return this;
		}
		public void Dispose()
		{
		}
	}
}
