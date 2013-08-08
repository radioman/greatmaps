using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	internal class ClockCacheRecord : CacheRecord
	{
		public bool touched = true;
		public ClockCacheRecord(IFuture future) : base(future)
		{
		}
	}
}
