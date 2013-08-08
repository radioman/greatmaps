using MSR.CVE.BackMaker.ImagePipeline;
using System;
namespace MSR.CVE.BackMaker
{
	public class OneLayerBoundApplier
	{
		internal IRenderableSource source;
		internal IFuturePrototype clippedImageFuture;
		internal Layer layer;
		internal OneLayerBoundApplier(IRenderableSource source, Layer layer, CachePackage cachePackage)
		{
			this.source = source;
			this.layer = layer;
			this.clippedImageFuture = new MemCachePrototype(cachePackage.computeCache, new ApplyPrototype(new UserClipperVerb(), new IFuturePrototype[]
			{
				source.GetImagePrototype(null, (FutureFeatures)11),
				new UnevaluatedTerm(TermName.TileAddress),
				source.GetUserBounds(null, FutureFeatures.Cached)
			}));
		}
		internal string DescribeSourceForComplaint()
		{
			return string.Format("Layer {0} Source {1}", this.layer.GetDisplayName(), this.source.GetSourceMapDisplayName());
		}
	}
}
