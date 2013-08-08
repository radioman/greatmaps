using System;
using System.Drawing;
namespace MSR.CVE.BackMaker
{
	public class RenderBounds
	{
		public int MinZoom;
		public int MaxZoom;
		public Size TileSize;
		public MapRectangle imageBounds;
		public TileRectangle[] tileRectangle;
	}
}
