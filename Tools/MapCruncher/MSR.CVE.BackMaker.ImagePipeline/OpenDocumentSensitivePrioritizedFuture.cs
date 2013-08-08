using System;
using System.Threading;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class OpenDocumentSensitivePrioritizedFuture : FutureBase, IDisposable
	{
		private OpenDocumentSensitivePrioritizer prioritizer;
		private IFuture future;
		private IFuture openDocumentFuture;
		private bool realizing;
		private int _identity;
		private static int nextIdentity = 0;
		private static object nextIdentityMutex = new object();
		private AsyncRef activeAsyncRef;
		public int identity
		{
			get
			{
				return this._identity;
			}
		}
		public OpenDocumentSensitivePrioritizedFuture(OpenDocumentSensitivePrioritizer prioritizer, IFuture future, IFuture openDocumentFuture)
		{
			this.prioritizer = prioritizer;
			this.future = future;
			this.openDocumentFuture = openDocumentFuture;
			object obj;
			Monitor.Enter(obj = OpenDocumentSensitivePrioritizedFuture.nextIdentityMutex);
			try
			{
				this._identity = OpenDocumentSensitivePrioritizedFuture.nextIdentity;
				OpenDocumentSensitivePrioritizedFuture.nextIdentity++;
			}
			finally
			{
				Monitor.Exit(obj);
			}
		}
		public override Present Realize(string refCredit)
		{
			Monitor.Enter(this);
			Present result;
			try
			{
				if (this.activeAsyncRef != null)
				{
					result = this.activeAsyncRef.Duplicate(refCredit);
				}
				else
				{
					D.Assert(!this.realizing);
					this.realizing = true;
					this.activeAsyncRef = (AsyncRef)this.future.Realize("ODSPF");
					AsyncRef asyncRef = (AsyncRef)this.activeAsyncRef.Duplicate(refCredit);
					this.prioritizer.Realizing(this);
					this.activeAsyncRef.AddCallback(new AsyncRecord.CompleteCallback(this.AsyncCompleteCallback));
					result = asyncRef;
				}
			}
			finally
			{
				Monitor.Exit(this);
			}
			return result;
		}
		public override void AccumulateRobustHash(IRobustHash hash)
		{
			hash.Accumulate("ODSPF(");
			this.future.AccumulateRobustHash(hash);
			hash.Accumulate(")");
		}
		private void AsyncCompleteCallback(AsyncRef asyncRef)
		{
			Monitor.Enter(this);
			try
			{
				this.realizing = false;
				this.prioritizer.Complete(this);
				this.activeAsyncRef.Dispose();
				this.activeAsyncRef = null;
			}
			finally
			{
				Monitor.Exit(this);
			}
		}
		internal IFuture GetOpenDocumentFuture()
		{
			return this.openDocumentFuture;
		}
		internal void DocumentStateChanged(bool isOpen)
		{
			this.activeAsyncRef.SetInterest(isOpen ? 524291 : 0);
		}
		public void Dispose()
		{
			Monitor.Enter(this);
			try
			{
				if (this.activeAsyncRef != null)
				{
					this.activeAsyncRef.Dispose();
					this.activeAsyncRef = null;
				}
			}
			finally
			{
				Monitor.Exit(this);
			}
		}
	}
}
