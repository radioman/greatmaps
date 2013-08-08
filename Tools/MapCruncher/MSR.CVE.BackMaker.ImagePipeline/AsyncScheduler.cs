using System;
using System.Collections.Generic;
using System.Threading;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class AsyncScheduler : IDisposable
	{
		private MemoryCache asyncRecordCache;
		private QueuedTileProvider qtp;
		public AsyncScheduler(int numWorkerThreads, string debugName)
		{
			this.asyncRecordCache = new AsyncRecordCache(debugName + "-Coalesce", false);
			this.qtp = new QueuedTileProvider(numWorkerThreads, debugName);
		}
		public void Dispose()
		{
			this.qtp.Dispose();
		}
		internal void Activate(LinkedList<AsyncRef> refs)
		{
			Monitor.Enter(this);
			try
			{
				List<QueueRequestIfc> list = new List<QueueRequestIfc>();
				foreach (AsyncRef current in refs)
				{
					if (current.asyncRecord.PrepareToQueue())
					{
						current.asyncRecord.AddCallback(new AsyncRecord.CompleteCallback(this.EvictFromCache));
						list.Add(current.asyncRecord.GetQTPRef());
					}
				}
				this.qtp.enqueueTileRequests(list.ToArray());
				D.Sayf(10, "PriQueue: {0}", new object[]
				{
					this.qtp
				});
			}
			finally
			{
				Monitor.Exit(this);
			}
		}
		internal MemoryCache GetCache()
		{
			return this.asyncRecordCache;
		}
		internal void EvictFromCache(AsyncRef aref)
		{
			this.asyncRecordCache.Evict(aref.asyncRecord.cacheKeyToEvict);
		}
		internal void ChangePriority(AsyncRef asyncRef)
		{
			this.qtp.ChangePriority(asyncRef);
		}
		public void Clear()
		{
			this.qtp.Clear();
		}
	}
}
