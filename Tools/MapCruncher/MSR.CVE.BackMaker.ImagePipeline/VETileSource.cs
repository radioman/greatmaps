using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class VETileSource : IRenderableSource, IComparable, IDisplayableSource
	{
		private CachePackage cachePackage;
		private string veStyle;
		private CoordinateSystemIfc coordinateSystem;
		public VETileSource(CachePackage cachePackage, string veStyle)
		{
			this.cachePackage = cachePackage;
			this.veStyle = veStyle;
			this.coordinateSystem = new MercatorCoordinateSystem();
		}
		public CoordinateSystemIfc GetDefaultCoordinateSystem()
		{
			return this.coordinateSystem;
		}
		public string GetRendererCredit()
		{
			return null;
		}
		public IFuture GetUserBounds(LatentRegionHolder latentRegionHolder, FutureFeatures features)
		{
			D.Assert(UnwarpedMapTileSource.HasFeature(features, FutureFeatures.Cached));
			IFuture future = new MemCacheFuture(this.cachePackage.boundsCache, new ApplyFuture(new ConstantVerb(new BoundsPresent(new RenderRegion(new MapRectangle(-85.0, -5000.0, 85.0, 5000.0), new DirtyEvent()))), new IFuture[0]));
			if (UnwarpedMapTileSource.HasFeature(features, FutureFeatures.Async))
			{
				future = new MemCacheFuture(this.cachePackage.asyncCache, Asynchronizer.MakeFuture(this.cachePackage.networkAsyncScheduler, future));
			}
			return future;
		}
		public IFuturePrototype GetImagePrototype(ImageParameterTypeIfc parameterType, FutureFeatures features)
		{
			D.Assert(parameterType == null);
			IFuturePrototype futurePrototype = new ApplyPrototype(new VETileFetch(this.veStyle), new IFuturePrototype[]
			{
				new UnevaluatedTerm(TermName.TileAddress)
			});
			futurePrototype = VETileSource.AddFeatures(futurePrototype, FutureFeatures.Cached & features, this.cachePackage);
			IFuturePrototype prototype = new ApplyPrototype(new VETileUpsamplerVerb(futurePrototype), new IFuturePrototype[]
			{
				new UnevaluatedTerm(TermName.TileAddress)
			});
			return VETileSource.AddFeatures(prototype, features, this.cachePackage);
		}
		public static IFuturePrototype AddFeatures(IFuturePrototype prototype, FutureFeatures features, CachePackage cachePackage)
		{
			if (UnwarpedMapTileSource.HasFeature(features, FutureFeatures.DiskCached))
			{
				prototype = new DiskCachePrototype(cachePackage.diskCache, prototype);
			}
			if (UnwarpedMapTileSource.HasFeature(features, FutureFeatures.Cached))
			{
				prototype = new MemCachePrototype(cachePackage.networkCache, prototype);
			}
			if (UnwarpedMapTileSource.HasFeature(features, FutureFeatures.Async))
			{
				prototype = new MemCachePrototype(cachePackage.asyncCache, new Asynchronizer(cachePackage.networkAsyncScheduler, prototype));
			}
			return prototype;
		}
		public string GetSourceMapDisplayName()
		{
			return string.Format("VE-{0}", this.veStyle);
		}
		public IFuture GetOpenDocumentFuture(FutureFeatures features)
		{
			return new FakeOpenDocumentFuture();
		}
		public int CompareTo(object obj)
		{
			if (!(obj is VETileSource))
			{
				return base.GetType().FullName.CompareTo(obj.GetType().FullName);
			}
			return this.veStyle.CompareTo(((VETileSource)obj).veStyle);
		}
	}
}
