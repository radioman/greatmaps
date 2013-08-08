using MSR.CVE.BackMaker.ImagePipeline;
using MSR.CVE.BackMaker.MCDebug;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
namespace MSR.CVE.BackMaker
{
	public class CompositeTileUnit : RenderWorkUnit
	{
		private Layer layer;
		private TileAddress address;
		private RenderOutputMethod renderOutput;
		private string outputFilename;
		private OutputTileType outputTileType;
		public int stage;
		private LinkedList<SingleSourceUnit> singleSourceUnits = new LinkedList<SingleSourceUnit>();
		private ITileWorkFeedback feedback;
		private ImageCodecInfo imageCodecInfo;

		//[CompilerGenerated]
		//private static Predicate<ImageCodecInfo> <>9__CachedAnonymousMethodDelegate1;
		
        public CompositeTileUnit(Layer layer, TileAddress address, RenderOutputMethod renderOutput, string outputFilename, OutputTileType outputTileType)
		{
			this.layer = layer;
			this.address = address;
			this.renderOutput = renderOutput;
			this.outputFilename = outputFilename;
			this.outputTileType = outputTileType;
		}
		public void AddSupplier(OneLayerBoundApplier applier)
		{
			this.singleSourceUnits.AddLast(new SingleSourceUnit(applier, this.address, this.stage, new SingleSourceUnit.NeedThisTileDelegate(this.NeedThisTile)));
		}
		public TileAddress GetTileAddress()
		{
			return this.address;
		}
		public bool NeedThisTile()
		{
			return !this.renderOutput.KnowFileExists(this.outputFilename);
		}
		public override bool DoWork(ITileWorkFeedback feedback)
		{
			this.feedback = feedback;
			bool result;
			try
			{
				D.Sayf(0, "{0} start compositing {1}", new object[]
				{
					Clocker.theClock.stamp(),
					this
				});
				if (!this.NeedThisTile())
				{
					D.Say(10, "Skipping extant file: " + this.outputFilename);
					result = false;
				}
				else
				{
					D.Sayf(10, "Compositing {0}", new object[]
					{
						this.address
					});
					Size tileSize = new MercatorCoordinateSystem().GetTileSize();
					GDIBigLockedImage gDIBigLockedImage = new GDIBigLockedImage(tileSize, "CompositeTileUnit");
					D.Say(10, string.Format("Start({0}) sm.count={1}", this.address, this.singleSourceUnits.Count));
					foreach (SingleSourceUnit current in this.singleSourceUnits)
					{
						current.CompositeImageInto(gDIBigLockedImage);
					}
					this.SaveTile(gDIBigLockedImage);
					result = true;
				}
			}
			catch (NonredundantRenderComplaint complaint)
			{
				feedback.PostComplaint(complaint);
				result = false;
			}
			catch (Exception arg)
			{
				feedback.PostMessage(string.Format("Exception compositing tile {0}: {1}", this.address, arg));
				result = false;
			}
			return result;
		}
		public override RenderWorkUnitComparinator GetWorkUnitComparinator()
		{
			return new RenderWorkUnitComparinator(new IComparable[]
			{
				this.address.ZoomLevel,
				this.layer.displayName,
				this.stage,
				1,
				this.address
			});
		}
		public override string ToString()
		{
			return string.Format("CTU layer {0} address {1} stage {2}", this.layer.displayName, this.address, this.stage);
		}
		private ImageCodecInfo SelectCodec()
		{
			if (this.imageCodecInfo == null)
			{
				List<ImageCodecInfo> list = new List<ImageCodecInfo>(ImageCodecInfo.GetImageDecoders());
				this.imageCodecInfo = list.Find((ImageCodecInfo info) => info.FormatDescription == "JPEG");
			}
			return this.imageCodecInfo;
		}
		private void SaveTile(GDIBigLockedImage compositeImage)
		{
			try
			{
				ImageRef imageRef = new ImageRef(new ImageRefCounted(compositeImage));
				GDIBigLockedImage.Transparentness transparentness = imageRef.image.GetTransparentness();
				if (transparentness == GDIBigLockedImage.Transparentness.EntirelyTransparent)
				{
					D.Sayf(0, "skipping blank tile.", new object[0]);
				}
				else
				{
					if (this.outputTileType == OutputTileType.IPIC)
					{
						if (transparentness == GDIBigLockedImage.Transparentness.EntirelyOpaque)
						{
							this.outputTileType = OutputTileType.JPG;
						}
						else
						{
							this.outputTileType = OutputTileType.PNG;
						}
					}
					RenderOutputUtil.SaveImage(imageRef, this.renderOutput, this.outputFilename, this.outputTileType.imageFormat);
				}
				this.feedback.PostImageResult(imageRef, this.layer, "(composite)", this.address);
				imageRef.Dispose();
			}
			catch (Exception arg)
			{
				this.feedback.PostMessage(string.Format("Can't create {0}: {1}", this.outputFilename, arg));
			}
		}
		internal IEnumerable<SingleSourceUnit> GetSingleSourceUnits()
		{
			return this.singleSourceUnits;
		}
	}
}
