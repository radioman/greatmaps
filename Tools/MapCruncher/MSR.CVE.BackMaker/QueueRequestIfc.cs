using System;
namespace MSR.CVE.BackMaker
{
	public interface QueueRequestIfc : RequestInterestIfc
	{
		string ToString();
		void DoWork();
		void DeQueued();
	}
}
