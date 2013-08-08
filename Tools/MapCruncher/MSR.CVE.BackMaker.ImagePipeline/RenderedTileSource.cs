using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class RenderedTileSource : IDisplayableSource
	{
		private CachePackage cachePackage;
		private RenderedTileNamingScheme namingScheme;
		private CoordinateSystemIfc coordinateSystem;
		public RenderedTileSource(CachePackage cachePackage, RenderedTileNamingScheme namingScheme)
		{
			this.cachePackage = cachePackage;
			this.namingScheme = namingScheme;
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
			D.Assert(UnwarpedMapTileSource.HasFeature(features, FutureFeatures.Async));
			return new MemCacheFuture(this.cachePackage.asyncCache, Asynchronizer.MakeFuture(this.cachePackage.computeAsyncScheduler, new MemCacheFuture(this.cachePackage.boundsCache, new ApplyFuture(new ConstantVerb(new BoundsPresent(new RenderRegion(new MapRectangle(-85.0, -5000.0, 85.0, 5000.0), new DirtyEvent()))), new IFuture[0]))));
		}
		public IFuturePrototype GetImagePrototype(ImageParameterTypeIfc parameterType, FutureFeatures features)
		{
			D.Assert(parameterType == null);
			IFuturePrototype prototype = new ApplyPrototype(new RenderedTileFetch(this.namingScheme), new IFuturePrototype[]
			{
				new UnevaluatedTerm(TermName.TileAddress)
			});
			return VETileSource.AddFeatures(prototype, features & (FutureFeatures)(-3), this.cachePackage);
		}
	}
}
