using System;
namespace MSR.CVE.BackMaker
{
	internal class QueueSuicideRequest : QueueRequestIfc, RequestInterestIfc
	{
		public void DoWork()
		{
		}
		public int GetInterest()
		{
			return -1;
		}
		public void DeQueued()
		{
			D.Assert(false, "Doesn't happen.");
		}
	}
}
