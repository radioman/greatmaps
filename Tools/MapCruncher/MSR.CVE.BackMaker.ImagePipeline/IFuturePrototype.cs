using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public interface IFuturePrototype
	{
		IFuture Curry(ParamDict paramDict);
	}
}
