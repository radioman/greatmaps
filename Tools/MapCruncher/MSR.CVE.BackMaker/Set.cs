using System;
using System.Collections.Generic;
namespace MSR.CVE.BackMaker
{
	public class Set<T> : Dictionary<T, bool>
	{
		public void Add(T item)
		{
			if (!base.ContainsKey(item))
			{
				base.Add(item, true);
			}
		}
	}
}
