using System;
namespace MSR.CVE.BackMaker.MCDebug
{
	internal class Clocker
	{
		private bool started;
		private DateTime startTime;
		public static Clocker theClock = new Clocker();
		public int stamp()
		{
			this.start();
			return (int)DateTime.Now.Subtract(this.startTime).TotalSeconds;
		}
		private void start()
		{
			if (this.started)
			{
				return;
			}
			this.startTime = DateTime.Now;
			this.started = true;
		}
	}
}
