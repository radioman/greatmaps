using MSR.CVE.BackMaker.MCDebug;
using System;
using System.Threading;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	internal abstract class CacheRecord
	{
		private IFuture future;
		private EventWaitHandle wait;
		internal Present _present;
		private int refs;
		private static ResourceCounter cacheRecordsCompletedResourceCounter = DiagnosticUI.theDiagnostics.fetchResourceCounter("cacheRecordsCompleted", -1);
		private static ResourceCounter cacheRecordsExtant = DiagnosticUI.theDiagnostics.fetchResourceCounter("cacheRecordsExtant", -1);
		internal Present present
		{
			get
			{
				return this._present;
			}
			set
			{
				int num = (this._present != null) ? 1 : 0;
				int num2 = (value != null) ? 1 : 0;
				CacheRecord.cacheRecordsCompletedResourceCounter.crement(num2 - num);
				this._present = value;
			}
		}
		public CacheRecord(IFuture future)
		{
			this.future = future;
			this.wait = new CountedEventWaitHandle(false, EventResetMode.ManualReset, "CacheRecord.Wait");
			this.refs = 1;
			CacheRecord.cacheRecordsExtant.crement(1);
		}
		public void Dispose()
		{
			if (this.present != null)
			{
				this.present.Dispose();
				this.present = null;
			}
			Monitor.Enter(this);
			try
			{
				if (this.wait != null)
				{
					this.wait.Close();
					this.wait = null;
				}
			}
			finally
			{
				Monitor.Exit(this);
			}
			CacheRecord.cacheRecordsExtant.crement(-1);
		}
		public void Process()
		{
			try
			{
				this.present = this.future.Realize("CacheRecord.Process");
			}
			catch (Exception ex)
			{
				this.present = new PresentFailureCode(ex);
			}
			D.Assert(this.present != null);
			Monitor.Enter(this);
			try
			{
				this.wait.Set();
				this.wait.Close();
				this.wait = null;
			}
			finally
			{
				Monitor.Exit(this);
			}
		}
		public Present WaitResult(string refCredit, string debugCacheName)
		{
			Monitor.Enter(this);
			WaitHandle waitHandle;
			try
			{
				waitHandle = this.wait;
			}
			finally
			{
				Monitor.Exit(this);
			}
			if (waitHandle != null)
			{
				waitHandle.WaitOne();
			}
			return this.Duplicate(refCredit);
		}
		public Present Duplicate(string refCredit)
		{
			return this.present.Duplicate(refCredit);
		}
		internal IFuture GetFuture()
		{
			return this.future;
		}
		public override string ToString()
		{
			if (this.present != null)
			{
				return "CacheRecord(present)";
			}
			return "CacheRecord(processing)";
		}
		internal void AddReference()
		{
			Monitor.Enter(this);
			try
			{
				this.refs++;
			}
			finally
			{
				Monitor.Exit(this);
			}
		}
		internal void DropReference()
		{
			Monitor.Enter(this);
			bool flag;
			try
			{
				this.refs--;
				flag = (this.refs == 0);
			}
			finally
			{
				Monitor.Exit(this);
			}
			if (flag)
			{
				this.Dispose();
			}
		}
	}
}
