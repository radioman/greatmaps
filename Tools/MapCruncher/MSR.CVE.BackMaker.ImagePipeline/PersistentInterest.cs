using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	internal class PersistentInterest
	{
		private InterestList interestList;
		public PersistentInterest(AsyncRef asyncRef)
		{
			this.interestList = new InterestList();
			this.interestList.Add(asyncRef);
			this.interestList.Activate();
			asyncRef.AddCallback(new AsyncRecord.CompleteCallback(this.AsyncCompleteCallback));
		}
		public void AsyncCompleteCallback(AsyncRef boundsAsyncRef)
		{
			this.interestList.Dispose();
		}
	}
}
