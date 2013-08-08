using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public interface Parameter : IRobustlyHashable, Present, IDisposable
	{
	}
}
