using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class RequestCanceledPresent : Present, IDisposable, IEvictable
	{
		public void Dispose()
		{
		}
		public Present Duplicate(string refCredit)
		{
			return this;
		}
		public bool EvictMeNow()
		{
			return true;
		}
	}
}
