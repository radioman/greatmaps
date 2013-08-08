using System;
using System.Collections.Generic;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class ParamDict : Dictionary<TermName, Parameter>
	{
		public ParamDict()
		{
		}
		public ParamDict(params object[] list)
		{
			int i;
			for (i = 0; i < list.Length; i += 2)
			{
				base[(TermName)list[i]] = (Parameter)((Parameter)list[i + 1]).Duplicate("Apply.Params");
			}
			D.Assert(i == list.Length, "unmatched pair in list.");
		}
	}
}
