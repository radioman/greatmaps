using System;
using System.Collections.Generic;
namespace MSR.CVE.BackMaker
{
	public class HeaderList : SortedList<string, string>
	{
		public HeaderList()
		{
		}
		public HeaderList(HeaderList prototype) : base(prototype)
		{
		}
		public void AddHeaderIfAbsent(string key, string value)
		{
			if (base.IndexOfKey(key) == -1)
			{
				base.Add(key, value);
			}
		}
	}
}
