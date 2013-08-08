using System;
namespace MSR.CVE.BackMaker
{
	public class NoView : ICurrentView
	{
		public object GetViewedObject()
		{
			return null;
		}
	}
}
