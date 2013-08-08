using System;
namespace MSR.CVE.BackMaker
{
	public class HeapBool
	{
		private bool b;
		public bool value
		{
			get
			{
				return this.b;
			}
			set
			{
				this.b = value;
			}
		}
		public HeapBool(bool initialValue)
		{
			this.b = initialValue;
		}
	}
}
