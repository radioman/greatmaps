using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public interface IFuture : IRobustlyHashable, IFuturePrototype
	{
		Present Realize(string refCredit);
	}
}
