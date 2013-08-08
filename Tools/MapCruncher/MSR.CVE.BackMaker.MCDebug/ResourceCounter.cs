using System;
namespace MSR.CVE.BackMaker.MCDebug
{
	public class ResourceCounter
	{
		public delegate void NotifyDelegate(ResourceCounter resourceCounter);
		private string resourceName;
		private int period;
		private int _value;
		private int lastRoundedValue;
		private ResourceCounter.NotifyDelegate notifyDelegate;
		public int Value
		{
			get
			{
				return this._value;
			}
		}
		public ResourceCounter(string resourceName, int period, ResourceCounter.NotifyDelegate notifyDelegate)
		{
			this.resourceName = resourceName;
			this.period = period;
			this.notifyDelegate = notifyDelegate;
		}
		public void crement(int crement)
		{
			this._value += crement;
			if (this.period > 0)
			{
				int num = this._value / this.period;
				if (Math.Abs(this.lastRoundedValue - num) > 1)
				{
					D.Sayf(0, "Resource {0} value {1}", new object[]
					{
						this.resourceName,
						this._value
					});
					this.lastRoundedValue = num;
				}
			}
			if (this.notifyDelegate != null)
			{
				this.notifyDelegate(this);
			}
		}
		internal void SetValue(int newValue)
		{
			this._value = newValue;
			if (this.notifyDelegate != null)
			{
				this.notifyDelegate(this);
			}
		}
	}
}
