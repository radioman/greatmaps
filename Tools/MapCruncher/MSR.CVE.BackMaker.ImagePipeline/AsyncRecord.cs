using System;
using System.Threading;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class AsyncRecord
	{
		public delegate void CompleteCallback(AsyncRef asyncRef);
		private AsyncScheduler scheduler;
		private IFuture _cacheKeyToEvict;
		private IFuture _future;
		private Present _present;
		private AsyncState asyncState;
		private int queuePriority;
		private AsyncRecord.CompleteCallback callbackEvent;
		private AsyncRef qtpRef;
		private AsyncRef notificationRef;
		private int refs;
		private bool disposed;
		public IFuture cacheKeyToEvict
		{
			get
			{
				return this._cacheKeyToEvict;
			}
		}
		public IFuture future
		{
			get
			{
				return this._future;
			}
		}
		public Present present
		{
			get
			{
				return this._present;
			}
		}
		public AsyncRecord(AsyncScheduler scheduler, IFuture cacheKeyToEvict, IFuture future)
		{
			this._cacheKeyToEvict = cacheKeyToEvict;
			this._future = future;
			this.scheduler = scheduler;
			this._present = null;
			this.asyncState = AsyncState.Prequeued;
			this.queuePriority = 0;
			this.qtpRef = new AsyncRef(this, "qRef");
		}
		public void AddCallback(AsyncRecord.CompleteCallback callback)
		{
			Monitor.Enter(this);
			try
			{
				if (this.present != null)
				{
					AsyncRef asyncRef = new AsyncRef(this, "AsyncRecord.AddCallback");
					callback(asyncRef);
					asyncRef.Dispose();
				}
				else
				{
					this.callbackEvent = (AsyncRecord.CompleteCallback)Delegate.Combine(this.callbackEvent, callback);
				}
			}
			finally
			{
				Monitor.Exit(this);
			}
		}
		public void Dispose()
		{
			D.Sayf(10, "Disposed({0})", new object[]
			{
				this
			});
			this._present.Dispose();
		}
		public void ChangePriority(int crement)
		{
			this.queuePriority += crement;
			if (this.qtpRef != null)
			{
				this.scheduler.ChangePriority(this.qtpRef);
			}
		}
		public AsyncRef GetQTPRef()
		{
			return this.qtpRef;
		}
		public void AddRef()
		{
			Monitor.Enter(this);
			try
			{
				D.Assert(!this.disposed);
				this.refs++;
			}
			finally
			{
				Monitor.Exit(this);
			}
		}
		public void DropRef()
		{
			Monitor.Enter(this);
			try
			{
				D.Assert(!this.disposed);
				this.refs--;
				if (this.refs == 0)
				{
					this.Dispose();
					this.disposed = true;
				}
			}
			finally
			{
				Monitor.Exit(this);
			}
		}
		internal bool PrepareToQueue()
		{
			Monitor.Enter(this);
			bool result;
			try
			{
				if (this.asyncState != AsyncState.Prequeued)
				{
					result = false;
				}
				else
				{
					this.asyncState = AsyncState.Queued;
					result = true;
				}
			}
			finally
			{
				Monitor.Exit(this);
			}
			return result;
		}
		internal int GetPriority()
		{
			return this.queuePriority;
		}
		internal AsyncScheduler GetScheduler()
		{
			return this.scheduler;
		}
		internal void DoWork()
		{
			D.Assert(this._present == null);
			Present presentValue;
			try
			{
				presentValue = this._future.Realize("AsyncRecord.DoWork");
			}
			catch (Exception ex)
			{
				presentValue = new PresentFailureCode(ex);
			}
			this.Notify(presentValue);
		}
		internal void DeQueued()
		{
			D.Say(10, string.Format("DeQueued({0})", this));
			this.Notify(new RequestCanceledPresent());
		}
		private void Notify(Present presentValue)
		{
			Monitor.Enter(this);
			try
			{
				D.Assert(this._present == null);
				this._present = presentValue;
				this.notificationRef = new AsyncRef(this, "callback");
				this.qtpRef = null;
			}
			finally
			{
				Monitor.Exit(this);
			}
			DebugThreadInterrupter.theInstance.AddThread("AsyncRecord.NotifyThread", new ThreadStart(this.NotifyThread), ThreadPriority.Normal);
		}
		private void NotifyThread()
		{
			this.callbackEvent(this.notificationRef);
			this.notificationRef.Dispose();
		}
		public override string ToString()
		{
			return "AsyncRecord:" + RobustHashTools.DebugString(this._future);
		}
		internal void ProcessSynchronously()
		{
			Monitor.Enter(this);
			try
			{
				if (this.asyncState == AsyncState.Queued)
				{
					return;
				}
				D.Assert(this.asyncState == AsyncState.Prequeued);
				this.asyncState = AsyncState.Queued;
			}
			finally
			{
				Monitor.Exit(this);
			}
			this.DoWork();
		}
	}
}
