using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class InjectedTileFailure : PresentFailureCode, IEvictable
	{
		public InjectedTileFailure() : base("Injected Tile Failure")
		{
		}
		public bool EvictMeNow()
		{
			return true;
		}
	}
}
