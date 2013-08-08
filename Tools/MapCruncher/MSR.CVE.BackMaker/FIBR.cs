using System;
using System.IO;
namespace MSR.CVE.BackMaker
{
	internal class FIBR
	{
		private StreamWriter sw;
		private static FIBR theFIBR = new FIBR();
		private FIBR()
		{
		}
		private void Dispose()
		{
			if (this.sw != null)
			{
				this.sw.Close();
				this.sw = null;
			}
		}
		private void Write(string message)
		{
			if (this.sw != null)
			{
				this.sw.WriteLine(message);
				this.sw.Flush();
			}
		}
		public static void Announce(string methodName, params object[] paramList)
		{
		}
	}
}
