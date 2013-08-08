using System;
using System.Collections.Generic;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class CompositeTileSource : IDisplayableSource
	{
		private Layer layer;
		private MapTileSourceFactory mapTileSourceFactory;
		public CompositeTileSource(Layer layer, MapTileSourceFactory mapTileSourceFactory)
		{
			this.layer = layer;
			this.mapTileSourceFactory = mapTileSourceFactory;
		}
		public CoordinateSystemIfc GetDefaultCoordinateSystem()
		{
			return MercatorCoordinateSystem.theInstance;
		}
		public string GetRendererCredit()
		{
			throw new Exception("The method or operation is not implemented.");
		}
		public IFuture GetUserBounds(LatentRegionHolder latentRegionHolder, FutureFeatures features)
		{
			List<IFuture> list = new List<IFuture>();
			foreach (SourceMap current in this.layer)
			{
				if (current.ReadyToLock())
				{
					LatentRegionHolder latentRegionHolder2 = new LatentRegionHolder(new DirtyEvent(), new DirtyEvent());
					list.Add(this.mapTileSourceFactory.CreateRenderableWarpedSource(current).GetUserBounds(latentRegionHolder2, features));
				}
			}
			IFuture future = new ApplyFuture(new CompositeBoundsVerb(), list.ToArray());
			D.Assert(UnwarpedMapTileSource.HasFeature(features, FutureFeatures.MemoryCached));
			return new MemCacheFuture(this.mapTileSourceFactory.GetCachePackage().boundsCache, future);
		}
		public IFuturePrototype GetImagePrototype(ImageParameterTypeIfc parameterType, FutureFeatures features)
		{
			List<IFuturePrototype> list = new List<IFuturePrototype>();
			list.Add(parameterType.GetSizeParameter());
			foreach (SourceMap current in this.layer)
			{
				if (current.ReadyToLock())
				{
					IDisplayableSource displayableSource = this.mapTileSourceFactory.CreateDisplayableWarpedSource(current);
					list.Add(displayableSource.GetImagePrototype(parameterType, features));
				}
			}
			IFuturePrototype futurePrototype = new ApplyPrototype(new CompositeImageVerb(), list.ToArray());
			if (UnwarpedMapTileSource.HasFeature(features, FutureFeatures.DiskCached))
			{
				futurePrototype = new DiskCachePrototype(this.mapTileSourceFactory.GetCachePackage().diskCache, futurePrototype);
			}
			if (UnwarpedMapTileSource.HasFeature(features, FutureFeatures.MemoryCached))
			{
				futurePrototype = new MemCachePrototype(this.mapTileSourceFactory.GetCachePackage().computeCache, futurePrototype);
			}
			return futurePrototype;
		}
	}
}
