using System;
namespace MSR.CVE.BackMaker
{
	public interface ParseableCfg
	{
		string name
		{
			get;
		}
		void ParseFrom(string str);
	}
}
