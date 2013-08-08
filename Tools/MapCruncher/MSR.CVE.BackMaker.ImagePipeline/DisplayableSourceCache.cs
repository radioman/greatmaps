using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	internal class DisplayableSourceCache : IDisplayableSource
	{
		private IDisplayableSource backingSource;
		private CoordinateSystemIfc cachedCoordSys;
		private string cachedRendererCredit;
		private LatentRegionHolder lastUserBoundsRequest_latentRegionHolder;
		private FutureFeatures lastUserBoundsRequest_features;
		private IFuture cachedUserBounds;
		private ImageParameterTypeIfc lastImageRequest_parameterType;
		private FutureFeatures lastImageRequest_features;
		private IFuturePrototype cachedImageRequest;
		public DisplayableSourceCache(IDisplayableSource backingSource)
		{
			this.backingSource = backingSource;
		}
		internal bool BackingStoreIs(IDisplayableSource backingSource)
		{
			return this.backingSource == backingSource;
		}
		public void Flush()
		{
			this.cachedCoordSys = null;
			this.cachedRendererCredit = null;
			this.cachedUserBounds = null;
			this.cachedImageRequest = null;
		}
		public CoordinateSystemIfc GetDefaultCoordinateSystem()
		{
			if (this.cachedCoordSys == null)
			{
				this.cachedCoordSys = this.backingSource.GetDefaultCoordinateSystem();
			}
			return this.cachedCoordSys;
		}
		public string GetRendererCredit()
		{
			if (this.cachedRendererCredit == null)
			{
				this.cachedRendererCredit = this.backingSource.GetRendererCredit();
			}
			return this.cachedRendererCredit;
		}
		public IFuture GetUserBounds(LatentRegionHolder latentRegionHolder, FutureFeatures features)
		{
			if (this.cachedUserBounds == null || this.lastUserBoundsRequest_latentRegionHolder != latentRegionHolder || this.lastUserBoundsRequest_features != features)
			{
				this.lastUserBoundsRequest_latentRegionHolder = latentRegionHolder;
				this.lastUserBoundsRequest_features = features;
				this.cachedUserBounds = this.backingSource.GetUserBounds(latentRegionHolder, features);
			}
			return this.cachedUserBounds;
		}
		public IFuturePrototype GetImagePrototype(ImageParameterTypeIfc parameterType, FutureFeatures features)
		{
			if (this.cachedImageRequest == null || this.lastImageRequest_parameterType != parameterType || this.lastImageRequest_features != features)
			{
				this.lastImageRequest_parameterType = parameterType;
				this.lastImageRequest_features = features;
				this.cachedImageRequest = this.backingSource.GetImagePrototype(parameterType, features);
			}
			return this.cachedImageRequest;
		}
	}
}
