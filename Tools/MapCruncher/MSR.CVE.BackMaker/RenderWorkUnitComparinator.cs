using System;
namespace MSR.CVE.BackMaker
{
	public struct RenderWorkUnitComparinator
	{
		public const int TYPE_SingleSourceUnit = 0;
		public const int TYPE_CompositeTileUnit = 1;
		public IComparable[] fields;
		public RenderWorkUnitComparinator(params IComparable[] fields)
		{
			this.fields = fields;
		}
	}
}
