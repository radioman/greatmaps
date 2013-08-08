using System;
using System.Collections.Generic;
namespace MSR.CVE.BackMaker.MCDebug
{
	internal class MakeObjectID
	{
		private Dictionary<WeakHashableObject, int> objectIDDict = new Dictionary<WeakHashableObject, int>();
		private int nextID;
		public static MakeObjectID Maker = new MakeObjectID();
		public int make(object o)
		{
			WeakHashableObject key = new WeakHashableObject(o);
			if (this.objectIDDict.ContainsKey(key))
			{
				return this.objectIDDict[key];
			}
			int num = this.nextID;
			this.nextID++;
			this.objectIDDict[key] = num;
			return num;
		}
	}
}
