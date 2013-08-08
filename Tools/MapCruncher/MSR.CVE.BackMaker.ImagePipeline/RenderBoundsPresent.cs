using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class RenderBoundsPresent : Present, IDisposable, IBoundsProvider
	{
		private RenderBounds renderBounds;
		public RenderBoundsPresent(RenderBounds renderBounds)
		{
			this.renderBounds = renderBounds;
		}
		public RenderBounds GetRenderBounds()
		{
			return this.renderBounds;
		}
		public RenderRegion GetRenderRegion()
		{
			return new RenderRegion(this.renderBounds.imageBounds, new DirtyEvent());
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
