using MSR.CVE.BackMaker.ImagePipeline;
using MSR.CVE.BackMaker.MCDebug;
using System;
using System.Drawing;
namespace MSR.CVE.BackMaker
{
	public class SingleSourceUnit : RenderWorkUnit
	{
		public delegate bool NeedThisTileDelegate();
		private OneLayerBoundApplier applier;
		private TileAddress address;
		private int stage;
		private SingleSourceUnit.NeedThisTileDelegate needThisTile;
		private static int debug_lastZoom = -1;
		internal SingleSourceUnit(OneLayerBoundApplier applier, TileAddress address, int stage, SingleSourceUnit.NeedThisTileDelegate needThisTile)
		{
			this.applier = applier;
			this.address = address;
			this.stage = stage;
			this.needThisTile = needThisTile;
		}
		public override bool DoWork(ITileWorkFeedback feedback)
		{
			if (!this.needThisTile())
			{
				return false;
			}
			D.Sayf(10, "SingleSourcing {0} {1}", new object[]
			{
				this.applier.source.GetSourceMapDisplayName(),
				this.address
			});
			Present present = this.FetchClippedImage();
			if (present is ImageRef)
			{
				ImageRef image = (ImageRef)present;
				feedback.PostImageResult(image, this.applier.layer, this.applier.source.GetSourceMapDisplayName(), this.address);
			}
			present.Dispose();
			return true;
		}
		public void CompositeImageInto(GDIBigLockedImage baseImage)
		{
			Present present = this.FetchClippedImage();
			if (present is ImageRef)
			{
				ImageRef imageRef = (ImageRef)present;
				baseImage.DrawImageOntoThis(imageRef.image, new Rectangle(0, 0, baseImage.Width, baseImage.Height), new Rectangle(0, 0, imageRef.image.Width, imageRef.image.Height));
			}
			else
			{
				if (present is PresentFailureCode)
				{
					throw new NonredundantRenderComplaint(string.Format("{0}: {1}", this.applier.DescribeSourceForComplaint(), ((PresentFailureCode)present).exception.Message));
				}
			}
			present.Dispose();
		}
		private Present FetchClippedImage()
		{
			if (SingleSourceUnit.debug_lastZoom != this.address.ZoomLevel)
			{
				SingleSourceUnit.debug_lastZoom = this.address.ZoomLevel;
				D.Sayf(0, "{0} start zoom level {1}", new object[]
				{
					Clocker.theClock.stamp(),
					this.address.ZoomLevel
				});
			}
			IFuture future = this.applier.clippedImageFuture.Curry(new ParamDict(new object[]
			{
				TermName.TileAddress,
				this.address
			}));
			return future.Realize("ImageRef.FetchClippedImage");
		}
		public override RenderWorkUnitComparinator GetWorkUnitComparinator()
		{
			return new RenderWorkUnitComparinator(new IComparable[]
			{
				this.address.ZoomLevel,
				this.applier.layer.displayName,
				this.stage,
				0,
				this.applier.source,
				this.address
			});
		}
		public override string ToString()
		{
			return string.Format("SSU layer {0} sm {1} address {2} stage {3}", new object[]
			{
				this.applier.layer.displayName,
				this.applier.source.GetSourceMapDisplayName(),
				this.address,
				this.stage
			});
		}
	}
}
