using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class CachePackage : IDisposable
	{
		private const string RootIdentifier = "root";
		public string identifier;
		public AsyncScheduler computeAsyncScheduler;
		public AsyncScheduler networkAsyncScheduler;
		public SizeSensitiveCache openSourceDocumentCache;
		public OpenDocumentSensitivePrioritizer openDocumentPrioritizer;
		public MemoryCache computeCache;
		public MemoryCache networkCache;
		public MemoryCache boundsCache;
		public MemoryCache asyncCache;
		public MemoryCache documentFetchCache;
		public DiskCache diskCache;
		private string suffix
		{
			get
			{
				return "-" + this.identifier;
			}
		}
		public CachePackage()
		{
			this.identifier = "root";
			this.Flush();
			this.openSourceDocumentCache = new SizeSensitiveCache("openDocumentCache" + this.suffix);
			this.openDocumentPrioritizer = new OpenDocumentSensitivePrioritizer(this.openSourceDocumentCache);
			this.diskCache = new DiskCache();
		}
		private CachePackage(string identifier)
		{
			this.identifier = identifier;
		}
		public CachePackage DeriveCache(string identifier)
		{
			CachePackage cachePackage = new CachePackage(identifier);
			cachePackage.openSourceDocumentCache = this.openSourceDocumentCache;
			cachePackage.openDocumentPrioritizer = this.openDocumentPrioritizer;
			cachePackage.diskCache = this.diskCache;
			cachePackage.Flush();
			return cachePackage;
		}
		public void Flush()
		{
			this.PreflushDispose();
			this.computeAsyncScheduler = new AsyncScheduler(1, "computeScheduler" + this.suffix);
			this.networkAsyncScheduler = new AsyncScheduler(8, "networkScheduler" + this.suffix);
			this.computeCache = new MemoryCache("computeCache" + this.suffix, 100);
			this.networkCache = new MemoryCache("networkCache" + this.suffix);
			this.boundsCache = new MemoryCache("boundsCache" + this.suffix);
			this.asyncCache = new AsyncRecordCache("asyncCache" + this.suffix, true);
			this.documentFetchCache = new MemoryCache("documentCache" + this.suffix, 10000);
		}
		private void PreflushDispose()
		{
			if (this.computeAsyncScheduler != null)
			{
				this.computeAsyncScheduler.Dispose();
				this.networkAsyncScheduler.Dispose();
				this.computeCache.Dispose();
				this.networkCache.Dispose();
				this.boundsCache.Dispose();
				this.asyncCache.Dispose();
				this.documentFetchCache.Dispose();
			}
		}
		public void Dispose()
		{
			this.PreflushDispose();
			if (this.identifier == "root")
			{
				this.openSourceDocumentCache.Dispose();
				this.diskCache.Dispose();
			}
		}
		public void ClearSchedulers()
		{
			if (this.computeAsyncScheduler != null)
			{
				this.computeAsyncScheduler.Clear();
			}
			if (this.networkAsyncScheduler != null)
			{
				this.networkAsyncScheduler.Clear();
			}
		}
	}
}
