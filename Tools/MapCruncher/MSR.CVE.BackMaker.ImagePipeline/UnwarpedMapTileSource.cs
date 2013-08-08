using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class UnwarpedMapTileSource : IDisplayableSource, IDocumentSource
	{
		private CachePackage cachePackage;
		private IFuture localDocumentFuture;
		private SourceMap sourceMap;
		public UnwarpedMapTileSource(CachePackage cachePackage, IFuture localDocumentFuture, SourceMap sourceMap)
		{
			this.cachePackage = cachePackage;
			this.localDocumentFuture = localDocumentFuture;
			this.sourceMap = sourceMap;
		}
		public CoordinateSystemIfc GetDefaultCoordinateSystem()
		{
			return new ContinuousCoordinateSystem();
		}
		public string GetRendererCredit()
		{
			IFuture accessFuture = this.GetAccessFuture(AccessMethod.RendererCredit, FutureFeatures.Cached, new IFuture[0]);
			Present present = accessFuture.Realize("UnwarpedMapTileSource.GetRendererCredit");
			if (present is StringParameter)
			{
				return ((StringParameter)present).value;
			}
			return null;
		}
		private IFuture GetAccessFuture(AccessMethod accessMethod, FutureFeatures openDocFeatures, params IFuture[] methodParams)
		{
			IFuture[] array = new IFuture[2 + methodParams.Length];
			array[0] = this.GetOpenDocumentFuture(openDocFeatures);
			array[1] = new ConstantFuture(new IntParameter((int)accessMethod));
			Array.Copy(methodParams, 0, array, 2, methodParams.Length);
			return new ApplyFuture(new ApplyVerbPresent(), array);
		}
		private IFuturePrototype GetAccessPrototype(AccessMethod accessMethod, FutureFeatures openDocFeatures, params IFuturePrototype[] methodParams)
		{
			IFuturePrototype[] array = new IFuturePrototype[2 + methodParams.Length];
			array[0] = this.GetOpenDocumentFuture(openDocFeatures);
			array[1] = new ConstantFuture(new IntParameter((int)accessMethod));
			Array.Copy(methodParams, 0, array, 2, methodParams.Length);
			return new ApplyPrototype(new ApplyVerbPresent(), array);
		}
		public string GetSourceMapDisplayName()
		{
			return this.sourceMap.displayName;
		}
		public IFuture GetOpenDocumentFuture(FutureFeatures features)
		{
			IFuture future = new FetchDocumentFuture(this.localDocumentFuture);
			D.Assert(UnwarpedMapTileSource.HasFeature(features, FutureFeatures.MemoryCached));
			if (UnwarpedMapTileSource.HasFeature(features, FutureFeatures.MemoryCached))
			{
				future = new MemCacheFuture(this.cachePackage.openSourceDocumentCache, future);
			}
			D.Assert(!UnwarpedMapTileSource.HasFeature(features, FutureFeatures.Async));
			return future;
		}
		public IFuturePrototype GetImageDetailPrototype(FutureFeatures features)
		{
			IFuturePrototype prototype = this.GetAccessPrototype(AccessMethod.ImageDetail, FutureFeatures.Cached, new IFuturePrototype[]
			{
				new UnevaluatedTerm(TermName.ImageDetail)
			});
			if (UnwarpedMapTileSource.HasFeature(features, FutureFeatures.MemoryCached))
			{
				prototype = new MemCachePrototype(this.cachePackage.computeCache, prototype);
			}
			return this.AddAsynchrony(prototype, features);
		}
		public IFuture GetImageBounds(FutureFeatures features)
		{
			IFuture future = this.GetAccessFuture(AccessMethod.FetchBounds, FutureFeatures.Cached, new IFuture[0]);
			future = new MemCacheFuture(this.cachePackage.boundsCache, future);
			return this.AddAsynchrony(future, features);
		}
		public IFuture GetUserBounds(LatentRegionHolder latentRegionHolder, FutureFeatures features)
		{
			D.Assert(UnwarpedMapTileSource.HasFeature(features, FutureFeatures.MemoryCached));
			D.Assert(!UnwarpedMapTileSource.HasFeature(features, FutureFeatures.Transparency));
			if (latentRegionHolder == null)
			{
				latentRegionHolder = this.sourceMap.latentRegionHolder;
			}
			latentRegionHolder.RequestRenderRegion(this.GetImageBounds((FutureFeatures)7));
			IFuture future = new MemCacheFuture(this.cachePackage.boundsCache, new ApplyFuture(new UserBoundsRefVerb(latentRegionHolder, this.GetImageBounds(FutureFeatures.Cached)), new IFuture[0]));
			return this.AddAsynchrony(future, features);
		}
		public IFuturePrototype GetImagePrototype(ImageParameterTypeIfc parameterType, FutureFeatures features)
		{
			if (parameterType == null)
			{
				parameterType = new ImageParameterFromTileAddress(this.GetDefaultCoordinateSystem());
			}
			FutureFeatures openDocFeatures = FutureFeatures.Cached;
			IFuturePrototype accessPrototype = this.GetAccessPrototype(AccessMethod.Render, openDocFeatures, new IFuturePrototype[]
			{
				new UnevaluatedTerm(TermName.ImageBounds),
				new UnevaluatedTerm(TermName.OutputSize),
				new UnevaluatedTerm(TermName.UseDocumentTransparency),
				new UnevaluatedTerm(TermName.ExactColors)
			});
			Verb verb = new SourceImageDownsamplerVerb(this.AddCaching(accessPrototype, FutureFeatures.Cached));
			IFuturePrototype futurePrototype = new ApplyPrototype(verb, new IFuturePrototype[]
			{
				parameterType.GetBoundsParameter(),
				parameterType.GetSizeParameter(),
				new ConstantFuture(new BoolParameter(this.sourceMap.transparencyOptions.useDocumentTransparency)),
				new ConstantFuture(new BoolParameter(UnwarpedMapTileSource.HasFeature(features, FutureFeatures.ExactColors)))
			});
			if (UnwarpedMapTileSource.HasFeature(features, FutureFeatures.Transparency))
			{
				IFuturePrototype futurePrototype2 = new ApplyPrototype(verb, new IFuturePrototype[]
				{
					parameterType.GetBoundsParameter(),
					parameterType.GetSizeParameter(),
					new ConstantFuture(new BoolParameter(this.sourceMap.transparencyOptions.useDocumentTransparency)),
					new ConstantFuture(new BoolParameter(true))
				});
				if (UnwarpedMapTileSource.HasFeature(features, FutureFeatures.MemoryCached))
				{
					futurePrototype = this.AddCaching(futurePrototype, FutureFeatures.MemoryCached);
					futurePrototype2 = this.AddCaching(futurePrototype2, FutureFeatures.MemoryCached);
				}
				futurePrototype = new TransparencyPrototype(this.sourceMap.transparencyOptions, futurePrototype, futurePrototype2);
			}
			futurePrototype = this.AddCaching(futurePrototype, features);
			return this.AddAsynchrony(futurePrototype, features);
		}
		public static bool HasFeature(FutureFeatures features, FutureFeatures query)
		{
			return (features & query) > FutureFeatures.Raw;
		}
		internal IFuture AddAsynchrony(IFuture future, FutureFeatures features)
		{
			if (UnwarpedMapTileSource.HasFeature(features, FutureFeatures.Async))
			{
				future = new MemCacheFuture(this.cachePackage.asyncCache, new OpenDocumentSensitivePrioritizedFuture(this.cachePackage.openDocumentPrioritizer, Asynchronizer.MakeFuture(this.cachePackage.computeAsyncScheduler, future), this.GetOpenDocumentFuture(FutureFeatures.MemoryCached)));
			}
			return future;
		}
		internal IFuturePrototype AddCaching(IFuturePrototype prototype, FutureFeatures features)
		{
			if (UnwarpedMapTileSource.HasFeature(features, FutureFeatures.DiskCached))
			{
				prototype = new DiskCachePrototype(this.cachePackage.diskCache, prototype);
			}
			if (UnwarpedMapTileSource.HasFeature(features, FutureFeatures.MemoryCached))
			{
				prototype = new MemCachePrototype(this.cachePackage.computeCache, prototype);
			}
			return prototype;
		}
		internal IFuturePrototype AddAsynchrony(IFuturePrototype prototype, FutureFeatures features)
		{
			if (UnwarpedMapTileSource.HasFeature(features, FutureFeatures.Async))
			{
				D.Assert(UnwarpedMapTileSource.HasFeature(features, FutureFeatures.MemoryCached), "should always cache async stuff, I think.");
				prototype = new MemCachePrototype(this.cachePackage.asyncCache, new OpenDocumentSensitivePrioritizedPrototype(this.cachePackage.openDocumentPrioritizer, new Asynchronizer(this.cachePackage.computeAsyncScheduler, prototype), this.GetOpenDocumentFuture(FutureFeatures.MemoryCached)));
			}
			return prototype;
		}
		internal string GetDocumentFilename()
		{
			Present present = this.localDocumentFuture.Realize("UnwarpedMapTileSource.GetDocumentFilename");
			if (present is SourceDocument)
			{
				return ((SourceDocument)present).localDocument.GetFilesystemAbsolutePath();
			}
			throw new Exception("Unable to fetch document");
		}
		internal TransparencyOptions GetTransparencyOptions()
		{
			return this.sourceMap.transparencyOptions;
		}
	}
}
