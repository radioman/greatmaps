using MSR.CVE.BackMaker.ImagePipeline;
using System;
namespace MSR.CVE.BackMaker
{
	internal class LegendDisplayableSourceWrapper : IDisplayableSource
	{
		private IDisplayableSource underSource;
		private LatentRegionHolder replacementLatentRegionHolder;
		public LegendDisplayableSourceWrapper(IDisplayableSource underSource, LatentRegionHolder replacementLatentRegionHolder)
		{
			this.underSource = underSource;
			this.replacementLatentRegionHolder = replacementLatentRegionHolder;
		}
		public CoordinateSystemIfc GetDefaultCoordinateSystem()
		{
			return this.underSource.GetDefaultCoordinateSystem();
		}
		public string GetRendererCredit()
		{
			return this.underSource.GetRendererCredit();
		}
		public IFuture GetUserBounds(LatentRegionHolder latentRegionHolder, FutureFeatures features)
		{
			if (latentRegionHolder == null)
			{
				latentRegionHolder = this.replacementLatentRegionHolder;
			}
			return this.underSource.GetUserBounds(latentRegionHolder, features);
		}
		public IFuturePrototype GetImagePrototype(ImageParameterTypeIfc parameterType, FutureFeatures features)
		{
			FutureFeatures features2 = features & (FutureFeatures)(-9);
			return this.underSource.GetImagePrototype(parameterType, features2);
		}
	}
}
