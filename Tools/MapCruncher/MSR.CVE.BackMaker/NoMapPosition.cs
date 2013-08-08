using System;
namespace MSR.CVE.BackMaker
{
	public class NoMapPosition
	{
		private MapPosition p = new MapPosition(null);
		public MapPosition NoMapPositionDelegate()
		{
			return this.p;
		}
	}
}
