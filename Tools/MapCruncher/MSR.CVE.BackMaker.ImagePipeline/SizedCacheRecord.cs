using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	internal class SizedCacheRecord : CacheRecord
	{
		public bool knowCorrectSize;
		public long memoryCharge;
		public SizedCacheRecord(IFuture future) : base(future)
		{
		}
	}
}
