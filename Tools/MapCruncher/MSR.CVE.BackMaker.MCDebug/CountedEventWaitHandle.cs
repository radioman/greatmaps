using System;
using System.Threading;
namespace MSR.CVE.BackMaker.MCDebug
{
	internal class CountedEventWaitHandle : EventWaitHandle
	{
		private string name;
		public CountedEventWaitHandle(bool initialState, EventResetMode mode, string name) : base(initialState, mode)
		{
			this.name = name;
			DiagnosticUI.theDiagnostics.fetchResourceCounter("event-" + name, -1).crement(1);
		}
		public override void Close()
		{
			base.Close();
			DiagnosticUI.theDiagnostics.fetchResourceCounter("event-" + this.name, -1).crement(-1);
		}
	}
}
