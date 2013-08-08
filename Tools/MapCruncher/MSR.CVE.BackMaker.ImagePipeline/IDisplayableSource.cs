using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public interface IDisplayableSource
	{
		CoordinateSystemIfc GetDefaultCoordinateSystem();
		string GetRendererCredit();
		IFuture GetUserBounds(LatentRegionHolder latentRegionHolder, FutureFeatures features);
		IFuturePrototype GetImagePrototype(ImageParameterTypeIfc parameterType, FutureFeatures features);
	}
}
