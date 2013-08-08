using System;
namespace MSR.CVE.BackMaker
{
	internal class CfgString : Cfg<string>
	{
		public CfgString(string name, string defaultValue) : base(name, defaultValue)
		{
		}
		public override void ParseFrom(string str)
		{
			this.value = str;
		}
	}
}
