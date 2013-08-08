using MSR.CVE.BackMaker.ImagePipeline;
using System;
namespace MSR.CVE.BackMaker
{
	public class MapTileSourceFactory
	{
		private CachePackage cachePackage;
		public MapTileSourceFactory(CachePackage cachePackage)
		{
			this.cachePackage = cachePackage;
		}
		public IDisplayableSource CreateDisplayableUnwarpedSource(SourceMap sourceMap)
		{
			return this.CreateUnwarpedSource(sourceMap);
		}
		public UnwarpedMapTileSource CreateUnwarpedSource(SourceMap sourceMap)
		{
			return new UnwarpedMapTileSource(this.cachePackage, sourceMap.documentFuture.GetSynchronousFuture(this.cachePackage), sourceMap);
		}
		public IDisplayableSource CreateDisplayableWarpedSource(SourceMap sourceMap)
		{
			if (!sourceMap.ReadyToLock())
			{
				return null;
			}
			return this.CreateWarpedSource(sourceMap);
		}
		public IRenderableSource CreateRenderableWarpedSource(SourceMap sourceMap)
		{
			D.Assert(sourceMap.ReadyToLock());
			return this.CreateWarpedSource(sourceMap);
		}
		public WarpedMapTileSource CreateWarpedSource(SourceMap sourceMap)
		{
			return new WarpedMapTileSource(this.CreateUnwarpedSource(sourceMap), this.cachePackage, sourceMap);
		}
		public int GetOpenSourceDocumentCacheSpillCount()
		{
			return this.cachePackage.openSourceDocumentCache.GetSpillCount();
		}
		internal void PurgeOpenSourceDocumentCache()
		{
			this.cachePackage.openSourceDocumentCache.Purge();
		}
		internal CachePackage GetCachePackage()
		{
			return this.cachePackage;
		}
		public string[] GetKnownFileTypes()
		{
			return FetchDocumentFuture.GetKnownFileTypes();
		}
	}
}
