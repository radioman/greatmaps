using System;
namespace MSR.CVE.BackMaker
{
	public class MashupFileWarning
	{
		private string msg;
		public MashupFileWarning(string msg)
		{
			this.msg = msg;
		}
		public override string ToString()
		{
			return this.msg;
		}
	}
}
