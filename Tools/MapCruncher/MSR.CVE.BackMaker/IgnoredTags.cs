using System;
using System.Collections.Generic;
namespace MSR.CVE.BackMaker
{
	public class IgnoredTags : Dictionary<string, bool>
	{
		public override string ToString()
		{
			string text = "";
			foreach (KeyValuePair<string, bool> current in this)
			{
				text = text + current.Key + " ";
			}
			return text;
		}
		public void Add(string tagName)
		{
			base[tagName] = true;
		}
	}
}
