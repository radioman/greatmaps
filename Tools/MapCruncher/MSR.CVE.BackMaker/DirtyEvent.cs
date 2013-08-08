using System;
namespace MSR.CVE.BackMaker
{
	public class DirtyEvent
	{
		private DirtyListener eventList;
		public DirtyEvent()
		{
		}
		public DirtyEvent(DirtyEvent parentEvent)
		{
			this.Add(parentEvent);
		}
		public void SetDirty()
		{
			if (this.eventList != null)
			{
				this.eventList();
			}
		}
		public void Add(DirtyListener listener)
		{
			this.eventList = (DirtyListener)Delegate.Combine(this.eventList, listener);
		}
		public void Add(DirtyEvent listener)
		{
			this.Add(new DirtyListener(listener.SetDirty));
		}
		public void Remove(DirtyListener listener)
		{
			this.eventList = (DirtyListener)Delegate.Remove(this.eventList, listener);
		}
		public void Remove(DirtyEvent listener)
		{
			this.Remove(new DirtyListener(listener.SetDirty));
		}
	}
}
