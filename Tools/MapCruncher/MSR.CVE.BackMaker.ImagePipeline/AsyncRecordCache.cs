using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class AsyncRecordCache : MemoryCache
	{
		private bool keepClean;
		public AsyncRecordCache(string debugName, bool keepClean) : base(debugName)
		{
			this.keepClean = keepClean;
		}
		protected override void Clean()
		{
			if (this.keepClean)
			{
				base.Clean();
			}
		}
	}
}
