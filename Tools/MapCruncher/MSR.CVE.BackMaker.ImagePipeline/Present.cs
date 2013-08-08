using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public interface Present : IDisposable
	{
		Present Duplicate(string refCredit);
	}
}
