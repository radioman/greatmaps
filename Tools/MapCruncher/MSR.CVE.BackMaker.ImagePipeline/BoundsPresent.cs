using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class BoundsPresent : Present, IDisposable, IBoundsProvider
	{
		private RenderRegion _renderRegion;
		public BoundsPresent(RenderRegion renderRegion)
		{
			this._renderRegion = renderRegion;
		}
		public RenderRegion GetRenderRegion()
		{
			return this._renderRegion;
		}
		public Present Duplicate(string refCredit)
		{
			return this;
		}
		public void Dispose()
		{
		}
	}
}
