using System;
using System.IO;
using System.Threading;
namespace MSR.CVE.BackMaker
{
	public class NamedPipeClient : NamedPipeBase, IDisposable
	{
		public NamedPipeClient(string name)
		{
			DateTime now = DateTime.Now;
			while (true)
			{
				this.pipeHandle = NamedPipeBase.CreateFile("\\\\.\\pipe\\" + name, 3221225472u, 0u, new IntPtr(0), 3u, 0u, new IntPtr(0));
				if (!this.pipeHandle.IsInvalid)
				{
					return;
				}
				if (DateTime.Now.Subtract(now).TotalSeconds > 10.0)
				{
					break;
				}
				Thread.Sleep(200);
			}
			throw new IOException("Failed to connect to " + name);
		}
		public void Dispose()
		{
			this.pipeHandle.Close();
		}
	}
}
