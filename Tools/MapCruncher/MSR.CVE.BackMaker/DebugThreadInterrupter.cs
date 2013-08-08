using System;
using System.Collections.Generic;
using System.Threading;
namespace MSR.CVE.BackMaker
{
	public class DebugThreadInterrupter
	{
		private class ThreadRec
		{
			private Thread _thread;
			public Thread thread
			{
				get
				{
					return this._thread;
				}
			}
			public ThreadRec(Thread thread)
			{
				this._thread = thread;
			}
			public override string ToString()
			{
				return string.Format("{0} {1}", this._thread.ManagedThreadId, this._thread.Name);
			}
		}
		public class ThreadWrapper
		{
			private ThreadStart threadStart;
			private DebugThreadInterrupter debugThreadInterrupter;
			public ThreadWrapper(ThreadStart threadStart, DebugThreadInterrupter debugThreadInterrupter)
			{
				this.threadStart = threadStart;
				this.debugThreadInterrupter = debugThreadInterrupter;
			}
			public void DoWork()
			{
				this.threadStart();
				this.debugThreadInterrupter.UnregisterThread(Thread.CurrentThread);
			}
		}
		private SortedDictionary<int, DebugThreadInterrupter.ThreadRec> threadDict = new SortedDictionary<int, DebugThreadInterrupter.ThreadRec>();
		private bool quitFlag;
		private EventWaitHandle quitEvent = new EventWaitHandle(false, EventResetMode.AutoReset, "DebugThreadInterrupter");
		public static DebugThreadInterrupter theInstance = new DebugThreadInterrupter();
		public DebugThreadInterrupter()
		{
			this.AddThread("DebugThreadInterrupter", new ThreadStart(this.DoWork), ThreadPriority.Normal);
		}
		public void AddThread(string name, ThreadStart start, ThreadPriority priority)
		{
			DebugThreadInterrupter.ThreadWrapper @object = new DebugThreadInterrupter.ThreadWrapper(start, this);
			Thread thread = new Thread(new ThreadStart(@object.DoWork));
			thread.Priority = priority;
			thread.Name = name;
			this.RegisterThread(thread);
			thread.Start();
			D.Sayf(1, "Started thread {0}", new object[]
			{
				name
			});
		}
		public void Quit()
		{
			this.quitFlag = true;
			this.quitEvent.Set();
		}
		private void DoWork()
		{
			while (true)
			{
				int num = -1;
				this.quitEvent.WaitOne(1000, false);
				if (this.quitFlag)
				{
					break;
				}
				DebugThreadInterrupter.ThreadRec threadRec = null;
				Monitor.Enter(this);
				try
				{
					if (num >= 0)
					{
						threadRec = this.threadDict[num];
					}
				}
				finally
				{
					Monitor.Exit(this);
				}
				if (threadRec != null)
				{
					threadRec.thread.Interrupt();
				}
			}
		}
		internal void RegisterThread(Thread thread)
		{
			Monitor.Enter(this);
			try
			{
				this.threadDict[thread.ManagedThreadId] = new DebugThreadInterrupter.ThreadRec(thread);
			}
			finally
			{
				Monitor.Exit(this);
			}
		}
		internal void UnregisterThread(Thread thread)
		{
			Monitor.Enter(this);
			DebugThreadInterrupter.ThreadRec threadRec;
			try
			{
				threadRec = this.threadDict[thread.ManagedThreadId];
				this.threadDict.Remove(thread.ManagedThreadId);
			}
			finally
			{
				Monitor.Exit(this);
			}
			D.Sayf(1, "Exiting thread {0}", new object[]
			{
				threadRec
			});
		}
	}
}
