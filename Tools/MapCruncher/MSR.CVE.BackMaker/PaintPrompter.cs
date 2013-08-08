using MSR.CVE.BackMaker.MCDebug;
using System;
using System.Threading;
using System.Windows.Forms;
namespace MSR.CVE.BackMaker
{
	internal class PaintPrompter
	{
		private delegate void UpdateDelegate();
		private Control paintee;
		private EventWaitHandle prompt = new CountedEventWaitHandle(false, EventResetMode.AutoReset, "PaintPrompter.prompt");
		private bool exit;
		public PaintPrompter(Control paintee)
		{
			this.paintee = paintee;
			DebugThreadInterrupter.theInstance.AddThread("PaintPrompter", new ThreadStart(this.Run), ThreadPriority.Normal);
		}
		public void Dispose()
		{
			this.exit = true;
			this.Prompt();
		}
		public void Prompt()
		{
			this.prompt.Set();
		}
		private void Run()
		{
			while (true)
			{
				this.prompt.WaitOne();
				if (this.exit)
				{
					break;
				}
				this.paintee.Invalidate();
				PaintPrompter.UpdateDelegate method = new PaintPrompter.UpdateDelegate(this.paintee.Update);
				this.paintee.Invoke(method);
			}
		}
	}
}
