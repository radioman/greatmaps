using System;
using System.Collections.Generic;
namespace MSR.CVE.BackMaker
{
	public class DebugComparer<T> : IEqualityComparer<T>
	{
		public bool Equals(T x, T y)
		{
			return x.Equals(y);
		}
		public int GetHashCode(T obj)
		{
			return obj.GetHashCode();
		}
	}
}
