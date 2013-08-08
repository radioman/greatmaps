using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class MapRectangleParameter : HashableImmutableParameter<MapRectangle>
	{
		public MapRectangleParameter(MapRectangle value) : base(value)
		{
		}
	}
}
