using System;
using System.Collections.Generic;
using System.Threading;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class SizeSensitiveCache : CacheBase
	{
		public const long paramUnknownObjectSizeWAG = 83886080L;
		private LinkedList<CacheRecord> lruQueue;
		private List<OpenDocumentStateObserverIfc> observers = new List<OpenDocumentStateObserverIfc>();
		private static bool oneEntryAtATime = false;
		private static long paramCacheMaxSize = (long)((SizeSensitiveCache.oneEntryAtATime ? 1 : 400) * 1048576);
		private long memoryUsed;
		private int spillCount;
		public SizeSensitiveCache(string debugName) : base(debugName)
		{
			this.lruQueue = new LinkedList<CacheRecord>();
		}
		internal override CacheRecord NewRecord(IFuture future)
		{
			return new SizedCacheRecord(future);
		}
		private void UpdateSizeForRecord(SizedCacheRecord record)
		{
			if (record.knowCorrectSize)
			{
				return;
			}
			this.memoryUsed -= record.memoryCharge;
			if (record.present != null && record.present is SizedObject)
			{
				record.memoryCharge = ((SizedObject)record.present).GetSize();
			}
			else
			{
				record.memoryCharge = 83886080L;
			}
			this.memoryUsed += record.memoryCharge;
		}
		internal override void Touch(CacheRecord record, bool recordIsNew)
		{
			if (!recordIsNew)
			{
				this.lruQueue.Remove(record);
			}
			this.UpdateSizeForRecord((SizedCacheRecord)record);
			this.lruQueue.AddLast(record);
		}
		internal override void TouchAfterUnlocked(CacheRecord record, bool recordIsNew)
		{
			if (recordIsNew)
			{
				this.NotifyObservers(record.GetFuture(), true);
			}
		}
		internal override void Remove(CacheRecord record, CacheBase.RemoveExpectation removeExpectation)
		{
			this.memoryUsed -= ((SizedCacheRecord)record).memoryCharge;
			this.lruQueue.Remove(record);
			base.Remove(record, removeExpectation);
		}
		protected override void Clean()
		{
			this.Clean(false);
		}
		public override Present Get(IFuture future, string refCredit)
		{
			if (SizeSensitiveCache.oneEntryAtATime)
			{
				Present present = base.Lookup(future);
				if (present != null)
				{
					return present;
				}
			}
			return base.Get(future, refCredit);
		}
		private void Clean(bool purge)
		{
			int num = 0;
			long num2 = this.memoryUsed;
			List<CacheRecord> list = new List<CacheRecord>();
			Monitor.Enter(this);
			try
			{
				while ((purge && this.lruQueue.Count > 0) || (this.memoryUsed > SizeSensitiveCache.paramCacheMaxSize && ((SizeSensitiveCache.oneEntryAtATime && this.lruQueue.Count > 0) || (!SizeSensitiveCache.oneEntryAtATime && this.lruQueue.Count > 1))))
				{
					CacheRecord value = this.lruQueue.First.Value;
					num++;
					list.Add(value);
					this.Remove(value, CacheBase.RemoveExpectation.Present);
					this.spillCount++;
				}
			}
			finally
			{
				Monitor.Exit(this);
			}
			foreach (CacheRecord current in list)
			{
				this.NotifyObservers(current.GetFuture(), false);
			}
			D.Sayf(10, "SizeSensitive Cleaner: removed {0} objects; from {1} to {2} MB", new object[]
			{
				num,
				num2 >> 20,
				this.memoryUsed >> 20
			});
		}
		internal int GetSpillCount()
		{
			return this.spillCount;
		}
		internal void Purge()
		{
			this.Clean(true);
			this.spillCount = 0;
		}
		internal void AddCallback(OpenDocumentStateObserverIfc openDocumentStateObserver)
		{
			this.observers.Add(openDocumentStateObserver);
		}
		private void NotifyObservers(IFuture openDocumentFuture, bool documentState)
		{
			foreach (OpenDocumentStateObserverIfc current in this.observers)
			{
				current.DocumentStateChanged(openDocumentFuture, documentState);
			}
		}
	}
}
