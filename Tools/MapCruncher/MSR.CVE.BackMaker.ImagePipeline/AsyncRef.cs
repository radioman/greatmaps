using MSR.CVE.BackMaker.MCDebug;
using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class AsyncRef : Present, IDisposable, QueueRequestIfc, RequestInterestIfc, IEvictable
	{
		public const int INTEREST_LEVEL_MORE_THAN_ANY_TILE = 524288;
		public const int INTEREST_LEVEL_OPEN_DOCUMENT_BONUS = 524291;
		public const int INTEREST_LEVEL_RENDER_ACTIVE_PAINT_EPSILON = 524296;
		public const int INTEREST_LEVEL_BOUNDS = 524290;
		public const int INTEREST_LEVEL_DOCUMENT = 524292;
		private AsyncRecord resource;
		private int localInterest;
		private string debugAnnotation;
		private bool debugDisposed;
		private static ResourceCounter asyncRefsHoldingInterestResourceCounter = DiagnosticUI.theDiagnostics.fetchResourceCounter("asyncRefsHoldingInterest-count", -1);
		public Present present
		{
			get
			{
				return this.resource.present;
			}
		}
		public IFuture future
		{
			get
			{
				return this.resource.future;
			}
		}
		internal AsyncRecord asyncRecord
		{
			get
			{
				return this.resource;
			}
		}
		public AsyncRef(AsyncRecord resource, string debugAnnotation)
		{
			this.resource = resource;
			this.debugAnnotation = debugAnnotation;
			DiagnosticUI.theDiagnostics.fetchResourceCounter("asyncRef-" + debugAnnotation, -1).crement(1);
			resource.AddRef();
		}
		public void Dispose()
		{
			D.Assert(!this.debugDisposed);
			this.SetInterest(0);
			this.resource.DropRef();
			DiagnosticUI.theDiagnostics.fetchResourceCounter("asyncRef-" + this.debugAnnotation, -1).crement(-1);
			this.debugDisposed = true;
		}
		public Present Duplicate(string refCredit)
		{
			return new AsyncRef(this.resource, refCredit);
		}
		public void SetInterest(int newInterest)
		{
			int num = (this.localInterest > 524291) ? 1 : 0;
			int num2 = (newInterest > 524291) ? 1 : 0;
			AsyncRef.asyncRefsHoldingInterestResourceCounter.crement(num2 - num);
			DiagnosticUI.theDiagnostics.fetchResourceCounter("asyncRef-" + this.debugAnnotation + "-withInterest", -1).crement(num2 - num);
			int crement = newInterest - this.localInterest;
			this.resource.ChangePriority(crement);
			this.localInterest = newInterest;
		}
		public void AddCallback(AsyncRecord.CompleteCallback callback)
		{
			this.resource.AddCallback(callback);
		}
		public override string ToString()
		{
			return this.asyncRecord.ToString();
		}
		public int GetInterest()
		{
			return this.asyncRecord.GetPriority();
		}
		public void DoWork()
		{
			this.asyncRecord.DoWork();
			this.Dispose();
		}
		public void DeQueued()
		{
			this.asyncRecord.DeQueued();
			this.Dispose();
		}
		internal void ProcessSynchronously()
		{
			this.asyncRecord.ProcessSynchronously();
		}
		public bool EvictMeNow()
		{
			return this.present is IEvictable && ((IEvictable)this.present).EvictMeNow();
		}
	}
}
