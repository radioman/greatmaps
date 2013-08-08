using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class RetryableFailure : PresentFailureCode, IEvictable
	{
		private DateTime retryTime;
		public RetryableFailure(Exception ex, string provenance) : base(new PresentFailureCode(ex), provenance)
		{
			this.retryTime = DateTime.Now.AddSeconds(15.0);
		}
		public bool EvictMeNow()
		{
			return DateTime.Now > this.retryTime;
		}
	}
}
