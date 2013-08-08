using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public interface ImageParameterTypeIfc
	{
		IFuturePrototype GetBoundsParameter();
		IFuturePrototype GetSizeParameter();
	}
}
