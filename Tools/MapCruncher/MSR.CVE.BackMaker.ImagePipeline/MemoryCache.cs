using MSR.CVE.BackMaker.MCDebug;
using System;
using System.Collections.Generic;
using System.Threading;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class MemoryCache : CacheBase
	{
		private const int paramCachedStuffGoal = 64;
		private LinkedList<CacheRecord> clockQueue;
		private int paramCacheMinSize = 64;
		private int nextCleanMark;
		private ResourceCounter interestingStuffResourceCounter;
		public MemoryCache(string debugName) : base(debugName)
		{
			this.clockQueue = new LinkedList<CacheRecord>();
			this.interestingStuffResourceCounter = DiagnosticUI.theDiagnostics.fetchResourceCounter("Cache:" + debugName + "-interestingStuff", -1);
		}
		public MemoryCache(string debugName, int paramCacheMinSize) : this(debugName)
		{
			this.paramCacheMinSize = paramCacheMinSize;
		}
		internal override CacheRecord NewRecord(IFuture future)
		{
			return new ClockCacheRecord(future);
		}
		internal override void Touch(CacheRecord record, bool recordIsNew)
		{
			if (recordIsNew)
			{
				this.clockQueue.AddLast(record);
			}
			((ClockCacheRecord)record).touched = true;
		}
		internal override void Remove(CacheRecord record, CacheBase.RemoveExpectation removeExpectation)
		{
			base.Remove(record, removeExpectation);
		}
		protected override void Clean()
		{
			int num = 0;
			int num2 = 0;
			Monitor.Enter(this);
			try
			{
				if (this.cache.Count >= this.nextCleanMark)
				{
					bool flag = false;
					int num3 = 0;
					for (int i = 0; i < this.clockQueue.Count; i++)
					{
						if (this.cache.Count < this.paramCacheMinSize)
						{
							flag = true;
							break;
						}
						ClockCacheRecord clockCacheRecord = (ClockCacheRecord)this.clockQueue.First.Value;
						this.clockQueue.RemoveFirst();
						num++;
						bool flag2 = clockCacheRecord.present is RequestInterestIfc && ((RequestInterestIfc)clockCacheRecord.present).GetInterest() > 524291;
						if (flag2)
						{
							num3++;
							this.clockQueue.AddLast(clockCacheRecord);
						}
						else
						{
							if (clockCacheRecord.touched)
							{
								clockCacheRecord.touched = false;
								this.clockQueue.AddLast(clockCacheRecord);
							}
							else
							{
								this.Remove(clockCacheRecord, CacheBase.RemoveExpectation.Unknown);
								num2++;
							}
						}
					}
					if (!flag)
					{
						int num4 = num3 * 2 + 64;
						if (num4 > this.paramCacheMinSize)
						{
							this.paramCacheMinSize = num4;
							D.Sayf(0, "Grew cache {0} to paramCacheMinSize={1}; cache now {2}; clock now {3}", new object[]
							{
								this.hashName,
								this.paramCacheMinSize,
								this.cache.Count,
								this.clockQueue.Count
							});
						}
					}
					this.interestingStuffResourceCounter.SetValue(num3);
					this.nextCleanMark = this.cache.Count + (this.paramCacheMinSize >> 2);
					D.Sayf(10, "MemoryCache Cleaner: removed {0}/{1}", new object[]
					{
						num2,
						num
					});
				}
			}
			finally
			{
				Monitor.Exit(this);
			}
		}
	}
}
