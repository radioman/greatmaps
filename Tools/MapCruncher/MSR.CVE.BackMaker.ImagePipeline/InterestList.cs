using MSR.CVE.BackMaker.MCDebug;
using System;
using System.Collections.Generic;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	internal class InterestList
	{
		private LinkedList<AsyncRef> list = new LinkedList<AsyncRef>();
		private static ResourceCounter globalInterestCounter = DiagnosticUI.theDiagnostics.fetchResourceCounter("interestList-activated-sum", -1);
		public void Add(AsyncRef aref)
		{
			this.list.AddLast(aref);
		}
		public void Activate()
		{
			Dictionary<AsyncScheduler, LinkedList<AsyncRef>> dictionary = new Dictionary<AsyncScheduler, LinkedList<AsyncRef>>();
			foreach (AsyncRef current in this.list)
			{
				AsyncScheduler scheduler = current.asyncRecord.GetScheduler();
				if (!dictionary.ContainsKey(scheduler))
				{
					dictionary[scheduler] = new LinkedList<AsyncRef>();
				}
				dictionary[scheduler].AddLast(current);
			}
			foreach (KeyValuePair<AsyncScheduler, LinkedList<AsyncRef>> current2 in dictionary)
			{
				current2.Key.Activate(current2.Value);
			}
			InterestList.globalInterestCounter.crement(this.list.Count);
		}
		public void Dispose()
		{
			foreach (AsyncRef current in this.list)
			{
				current.Dispose();
			}
			InterestList.globalInterestCounter.crement(-this.list.Count);
			this.list.Clear();
		}
	}
}
