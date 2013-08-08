using System;
namespace MSR.CVE.BackMaker
{
	public abstract class RenderWorkUnit : IComparable
	{
		public abstract bool DoWork(ITileWorkFeedback feedback);
		public abstract RenderWorkUnitComparinator GetWorkUnitComparinator();
		public int CompareTo(object obj)
		{
			RenderWorkUnitComparinator workUnitComparinator = this.GetWorkUnitComparinator();
			RenderWorkUnitComparinator workUnitComparinator2 = ((RenderWorkUnit)obj).GetWorkUnitComparinator();
			int num = Math.Min(workUnitComparinator.fields.Length, workUnitComparinator2.fields.Length);
			for (int i = 0; i < num; i++)
			{
				int num2 = workUnitComparinator.fields[i].CompareTo(workUnitComparinator2.fields[i]);
				if (num2 != 0)
				{
					return num2;
				}
			}
			return workUnitComparinator.fields.Length.CompareTo(workUnitComparinator2.fields.Length);
		}
	}
}
