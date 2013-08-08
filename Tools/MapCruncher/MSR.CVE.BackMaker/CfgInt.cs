using System;
namespace MSR.CVE.BackMaker
{
	internal class CfgInt : Cfg<int>
	{
		public CfgInt(string name, int defaultValue) : base(name, defaultValue)
		{
		}
		public override void ParseFrom(string str)
		{
			this.value = Convert.ToInt32(str);
		}
	}
}
