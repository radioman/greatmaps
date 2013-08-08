using System;
using System.Collections.Generic;
namespace MSR.CVE.BackMaker
{
	public class MashupFileWarningList : List<MashupFileWarning>
	{
		public override string ToString()
		{
			string text = "";
			foreach (MashupFileWarning current in this)
			{
				text = text + current + "\n";
			}
			return text;
		}
	}
}
