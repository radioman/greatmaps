using MSR.CVE.BackMaker.MCDebug;
using System;
using System.Collections.Generic;
using System.Threading;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public abstract class CacheBase : IRobustlyHashable
	{
		public enum RemoveExpectation
		{
			Absent,
			Present,
			Unknown
		}
		internal string hashName;
		internal Dictionary<IFuture, CacheRecord> cache;
		private ResourceCounter resourceCounter;
		public CacheBase(string hashName)
		{
			this.hashName = hashName;
			this.cache = new Dictionary<IFuture, CacheRecord>();
			this.resourceCounter = DiagnosticUI.theDiagnostics.fetchResourceCounter("Cache:" + hashName, -1);
		}
		public void Dispose()
		{
			Dictionary<IFuture, CacheRecord> dictionary = this.cache;
			this.cache = new Dictionary<IFuture, CacheRecord>();
			foreach (CacheRecord current in dictionary.Values)
			{
				this.Remove(current, CacheBase.RemoveExpectation.Absent);
			}
		}
		internal abstract void Touch(CacheRecord record, bool recordIsNew);
		internal virtual void TouchAfterUnlocked(CacheRecord record, bool recordIsNew)
		{
		}
		protected abstract void Clean();
		internal abstract CacheRecord NewRecord(IFuture future);
		internal virtual void Remove(CacheRecord record, CacheBase.RemoveExpectation removeExpectation)
		{
			Monitor.Enter(this);
			try
			{
				bool flag = this.cache.Remove(record.GetFuture());
				D.Assert(removeExpectation == CacheBase.RemoveExpectation.Unknown || removeExpectation == CacheBase.RemoveExpectation.Present == flag, "Remove didn't meet expectations. That could suggest a mutating hash.");
				this.resourceCounter.crement(-1);
				record.DropReference();
			}
			finally
			{
				Monitor.Exit(this);
			}
		}
		public Present Lookup(IFuture future)
		{
			Monitor.Enter(this);
			CacheRecord cacheRecord;
			try
			{
				if (!this.cache.ContainsKey(future))
				{
					return null;
				}
				cacheRecord = this.cache[future];
				this.Touch(cacheRecord, false);
				cacheRecord.AddReference();
			}
			finally
			{
				Monitor.Exit(this);
			}
			Present result = cacheRecord.WaitResult("lookup", this.hashName);
			cacheRecord.DropReference();
			return result;
		}
		public bool Contains(IFuture future)
		{
			Monitor.Enter(this);
			bool result;
			try
			{
				if (this.cache.ContainsKey(future))
				{
					this.Touch(this.cache[future], false);
					result = true;
				}
				else
				{
					result = false;
				}
			}
			finally
			{
				Monitor.Exit(this);
			}
			return result;
		}
		public virtual Present Get(IFuture future, string refCredit)
		{
			bool flag = false;
			bool flag2 = false;
			Present result;
			try
			{
				this.Clean();
				Monitor.Enter(this);
				CacheRecord cacheRecord;
				bool recordIsNew;
				try
				{
					if (!this.cache.ContainsKey(future))
					{
						cacheRecord = this.NewRecord(future);
						this.cache[future] = cacheRecord;
						this.resourceCounter.crement(1);
						flag = true;
						recordIsNew = true;
					}
					else
					{
						cacheRecord = this.cache[future];
						recordIsNew = false;
					}
					this.Touch(cacheRecord, recordIsNew);
					cacheRecord.AddReference();
				}
				finally
				{
					Monitor.Exit(this);
				}
				this.TouchAfterUnlocked(cacheRecord, recordIsNew);
				if (flag)
				{
					cacheRecord.Process();
					flag2 = true;
				}
				Present present = cacheRecord.WaitResult(refCredit, this.hashName);
				cacheRecord.DropReference();
				if (present is IEvictable && ((IEvictable)present).EvictMeNow())
				{
					D.Sayf(0, "Evicting canceled request for {0}", new object[]
					{
						future
					});
					this.Evict(future);
				}
				result = present;
			}
			finally
			{
				D.Assert(!flag || flag2);
			}
			return result;
		}
		internal void Evict(IFuture future)
		{
			Monitor.Enter(this);
			try
			{
				bool flag = this.cache.ContainsKey(future);
				if (flag)
				{
					CacheRecord record = this.cache[future];
					this.Remove(record, CacheBase.RemoveExpectation.Unknown);
				}
			}
			finally
			{
				Monitor.Exit(this);
			}
		}
		public void AccumulateRobustHash(IRobustHash hash)
		{
			hash.Accumulate(string.Format("Cache({0})", this.hashName));
		}
	}
}
