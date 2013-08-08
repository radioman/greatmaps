using MSR.CVE.BackMaker.ImagePipeline;
using System;
namespace MSR.CVE.BackMaker
{
	public class LatentRegionHolder
	{
		private DirtyEvent dirtyEvent;
		private DirtyEvent boundsChangedEvent;
		public RenderRegion renderRegion;
		public LatentRegionHolder(DirtyEvent parentEvent, DirtyEvent parentBoundsAvailableEvent)
		{
			this.dirtyEvent = parentEvent;
			this.boundsChangedEvent = parentBoundsAvailableEvent;
		}
		public RenderRegion GetRenderRegionSynchronously(IFuture synchronousImageBoundsFuture)
		{
			Present present = synchronousImageBoundsFuture.Realize("LatentRegionHolder.GetRenderRegionSynchronously");
			this.StoreRenderRegion(present);
			if (this.renderRegion == null)
			{
				throw new Exception("Render region request failed, gasp: " + present.ToString());
			}
			return this.renderRegion;
		}
		public void RequestRenderRegion(IFuture asynchronousImageBoundsFuture)
		{
			if (this.renderRegion == null)
			{
				AsyncRef asyncRef = (AsyncRef)asynchronousImageBoundsFuture.Realize("LatentRegionHolder.RequestRenderRegion");
				asyncRef.AddCallback(new AsyncRecord.CompleteCallback(this.ImageBoundsAvailable));
				new PersistentInterest(asyncRef);
			}
		}
		private void ImageBoundsAvailable(AsyncRef asyncRef)
		{
			this.StoreRenderRegion(asyncRef.present);
		}
		private void StoreRenderRegion(Present present)
		{
			if (this.renderRegion == null && present is BoundsPresent)
			{
				BoundsPresent boundsPresent = (BoundsPresent)present;
				this.renderRegion = boundsPresent.GetRenderRegion().Copy(this.dirtyEvent);
				this.boundsChangedEvent.SetDirty();
			}
		}
	}
}
