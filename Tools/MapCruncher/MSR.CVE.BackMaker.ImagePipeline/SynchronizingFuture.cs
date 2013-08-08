using MSR.CVE.BackMaker.MCDebug;
using System;
using System.Threading;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class SynchronizingFuture : FutureBase
	{
		private IFuture asyncFuture;
		private int interestValue;
		private EventWaitHandle asyncReadyEvent = new CountedEventWaitHandle(false, EventResetMode.ManualReset, "SynchronizingFuture.ReadyEvent");
		public SynchronizingFuture(IFuture asyncFuture, int interestValue)
		{
			this.asyncFuture = asyncFuture;
			this.interestValue = interestValue;
		}
		public void Dispose()
		{
			if (this.asyncReadyEvent != null)
			{
				this.asyncReadyEvent.Close();
				this.asyncReadyEvent = null;
			}
		}
		public override Present Realize(string refCredit)
		{
			Present present = this.asyncFuture.Realize(refCredit);
			if (present is AsyncRef)
			{
				AsyncRef asyncRef = (AsyncRef)present;
				asyncRef.AddCallback(new AsyncRecord.CompleteCallback(this.PresentReadyCallback));
				asyncRef.SetInterest(this.interestValue);
				AsyncRef asyncRef2 = (AsyncRef)asyncRef.Duplicate(refCredit + "2");
				new PersistentInterest(asyncRef);
				this.asyncReadyEvent.WaitOne();
				return asyncRef2.present;
			}
			return present;
		}
		private void PresentReadyCallback(AsyncRef asyncRef)
		{
			this.asyncReadyEvent.Set();
		}
		public override void AccumulateRobustHash(IRobustHash hash)
		{
			hash.Accumulate("SynchronizingFuture(");
			this.asyncFuture.AccumulateRobustHash(hash);
			hash.Accumulate(")");
		}
	}
}
