using System;
using System.Drawing;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class WarpedMapTileSource : IRenderableSource, IComparable, IDisplayableSource
	{
		private UnwarpedMapTileSource unwarpedMapTileSource;
		private CachePackage cachePackage;
		private MercatorCoordinateSystem coordinateSystem;
		private IImageTransformer imageTransformer;
		private static Size sourceImageOversampleSize = new Size(512, 512);
		public WarpedMapTileSource(UnwarpedMapTileSource unwarpedTileSource, CachePackage cachePackage, SourceMap sourceMap)
		{
			this.unwarpedMapTileSource = unwarpedTileSource;
			this.cachePackage = cachePackage;
			this.coordinateSystem = new MercatorCoordinateSystem();
			if (sourceMap.registration.GetAssociationList().Count < sourceMap.registration.warpStyle.getCorrespondencesRequired())
			{
				throw new InsufficientCorrespondencesException();
			}
			this.imageTransformer = sourceMap.registration.warpStyle.getImageTransformer(sourceMap.registration, RenderQualityStyle.theStyle.warpInterpolationMode);
		}
		public CoordinateSystemIfc GetDefaultCoordinateSystem()
		{
			return this.coordinateSystem;
		}
		public string GetRendererCredit()
		{
			return this.unwarpedMapTileSource.GetRendererCredit();
		}
		public IFuture GetUserBounds(LatentRegionHolder latentRegionHolder, FutureFeatures features)
		{
			IFuture future = new ApplyFuture(new WarpBoundsVerb(this.imageTransformer), new IFuture[]
			{
				this.unwarpedMapTileSource.GetUserBounds(latentRegionHolder, FutureFeatures.Cached)
			});
			future = new MemCacheFuture(this.cachePackage.boundsCache, future);
			return this.unwarpedMapTileSource.AddAsynchrony(future, features);
		}
		public IFuturePrototype GetImagePrototype(ImageParameterTypeIfc parameterType, FutureFeatures features)
		{
			if (parameterType == null)
			{
				parameterType = new ImageParameterFromTileAddress(this.GetDefaultCoordinateSystem());
			}
			FutureFeatures futureFeatures = FutureFeatures.Raw;
			if (UnwarpedMapTileSource.HasFeature(features, FutureFeatures.Transparency))
			{
				futureFeatures |= FutureFeatures.Transparency;
			}
			if (UnwarpedMapTileSource.HasFeature(features, FutureFeatures.MemoryCached))
			{
				futureFeatures |= FutureFeatures.MemoryCached;
			}
			IFuturePrototype futurePrototype = new ApplyPrototype(new WarpImageVerb(this.imageTransformer, this.GetImageBounds(FutureFeatures.Cached), this.unwarpedMapTileSource.GetImagePrototype(new ImageParameterFromRawBounds(WarpedMapTileSource.sourceImageOversampleSize), futureFeatures)), new IFuturePrototype[]
			{
				parameterType.GetBoundsParameter(),
				parameterType.GetSizeParameter()
			});
			if (parameterType is ImageParameterFromTileAddress)
			{
				futurePrototype = new ApplyPrototype(new FadeVerb(this.unwarpedMapTileSource.GetTransparencyOptions().GetFadeOptions()), new IFuturePrototype[]
				{
					futurePrototype,
					new UnevaluatedTerm(TermName.TileAddress)
				});
			}
			else
			{
				D.Say(2, "Warning: Ignoring fade options because I don't have a tile address.");
			}
			futurePrototype = this.unwarpedMapTileSource.AddCaching(futurePrototype, features);
			return this.unwarpedMapTileSource.AddAsynchrony(futurePrototype, features);
		}
		public string GetSourceMapDisplayName()
		{
			return this.unwarpedMapTileSource.GetSourceMapDisplayName();
		}
		public IFuture GetOpenDocumentFuture(FutureFeatures features)
		{
			return this.unwarpedMapTileSource.GetOpenDocumentFuture(features);
		}
		public int CompareTo(object obj)
		{
			if (!(obj is WarpedMapTileSource))
			{
				return base.GetType().FullName.CompareTo(obj.GetType().FullName);
			}
			return this.GetDocumentFilename().CompareTo(((WarpedMapTileSource)obj).GetDocumentFilename());
		}
		private string GetDocumentFilename()
		{
			return this.unwarpedMapTileSource.GetDocumentFilename();
		}
		public IFuture GetImageBounds(FutureFeatures features)
		{
			IFuture future = new ApplyFuture(new WarpBoundsVerb(this.imageTransformer), new IFuture[]
			{
				this.unwarpedMapTileSource.GetImageBounds(features)
			});
			future = new MemCacheFuture(this.cachePackage.boundsCache, future);
			return this.unwarpedMapTileSource.AddAsynchrony(future, features);
		}
		internal RegistrationDefinition ComputeWarpedRegistration()
		{
			return this.imageTransformer.getWarpedRegistration();
		}
		internal IPointTransformer GetDestLatLonToSourceTransformer()
		{
			return this.imageTransformer.getDestLatLonToSourceTransformer();
		}
		internal IPointTransformer GetSourceToDestLatLonTransformer()
		{
			return this.imageTransformer.getSourceToDestLatLonTransformer();
		}
	}
}
