using System;
namespace MSR.CVE.BackMaker
{
	public class DirtyString
	{
		private string _myValue;
		public DirtyEvent dirtyEvent;
		public string myValue
		{
			get
			{
				if (this._myValue == null)
				{
					return "";
				}
				return this._myValue;
			}
			set
			{
				if (value != this._myValue)
				{
					this._myValue = value;
					this.dirtyEvent.SetDirty();
				}
			}
		}
		public DirtyString(DirtyEvent parentDirtyEvent)
		{
			this.dirtyEvent = new DirtyEvent(parentDirtyEvent);
		}
	}
}
