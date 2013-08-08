using MSR.CVE.BackMaker.ImagePipeline;
using System;
using System.Drawing;
using System.Globalization;
namespace MSR.CVE.BackMaker
{
	public class Legend : PositionMemoryIfc, HasDisplayNameIfc, LastViewIfc
	{
		public class RenderFailedException : Exception
		{
			public RenderFailedException(string msg) : base(msg)
			{
			}
		}
		private const string displayNameAttr = "DisplayName";
		private const string renderedSizeAttr = "RenderedSize";
		public static RangeInt renderedSizeRange = new RangeInt(50, 1000);
		public DirtyEvent dirtyEvent;
		private SourceMap _sourceMap;
		private string _displayName;
		private LatentRegionHolder _latentRegionHolder;
		private int _renderedSize = 500;
		private LegendView _lastView;
		public LatentRegionHolder latentRegionHolder
		{
			get
			{
				return this._latentRegionHolder;
			}
		}
		public ICurrentView lastView
		{
			get
			{
				return this._lastView;
			}
		}
		public SourceMap sourceMap
		{
			get
			{
				return this._sourceMap;
			}
		}
		public int renderedSize
		{
			get
			{
				return this._renderedSize;
			}
			set
			{
				if (value != this._renderedSize)
				{
					this._renderedSize = value;
					this.dirtyEvent.SetDirty();
				}
			}
		}
		public string displayName
		{
			get
			{
				return this._displayName;
			}
			set
			{
				this._displayName = value;
				this.dirtyEvent.SetDirty();
			}
		}
		public Legend(SourceMap sourceMap, DirtyEvent parentEvent, DirtyEvent parentBoundsChangedEvent)
		{
			this._sourceMap = sourceMap;
			this.dirtyEvent = new DirtyEvent(parentEvent);
			this._latentRegionHolder = new LatentRegionHolder(this.dirtyEvent, parentBoundsChangedEvent);
			this._displayName = "legend";
		}
		public static string GetXMLTag()
		{
			return "Legend";
		}
		public Legend(SourceMap sourceMap, MashupParseContext context, DirtyEvent parentEvent, DirtyEvent parentBoundsChangedEvent)
		{
			this._sourceMap = sourceMap;
			this.dirtyEvent = new DirtyEvent(parentEvent);
			this._latentRegionHolder = new LatentRegionHolder(this.dirtyEvent, parentBoundsChangedEvent);
			this._displayName = context.GetRequiredAttribute("DisplayName");
			string attribute = context.reader.GetAttribute("RenderedSize");
			if (attribute != null)
			{
				Legend.renderedSizeRange.Parse(context, "RenderedSize", attribute);
			}
			XMLTagReader xMLTagReader = context.NewTagReader(Legend.GetXMLTag());
			context.ExpectIdentity(this);
			while (xMLTagReader.FindNextStartTag())
			{
				if (xMLTagReader.TagIs(RenderRegion.GetXMLTag()))
				{
					context.AssertUnique(this._latentRegionHolder.renderRegion);
					this._latentRegionHolder.renderRegion = new RenderRegion(context, this.dirtyEvent, ContinuousCoordinateSystem.theInstance);
				}
				else
				{
					if (xMLTagReader.TagIs(LegendView.GetXMLTag()))
					{
						this._lastView = new LegendView(this, context);
					}
				}
			}
		}
		public void WriteXML(MashupWriteContext context)
		{
			context.writer.WriteStartElement(Legend.GetXMLTag());
			context.WriteIdentityAttr(this);
			context.writer.WriteAttributeString("DisplayName", this._displayName);
			context.writer.WriteAttributeString("RenderedSize", this.renderedSize.ToString(CultureInfo.InvariantCulture));
			if (this._latentRegionHolder.renderRegion != null)
			{
				this._latentRegionHolder.renderRegion.WriteXML(context.writer);
			}
			if (this._lastView != null)
			{
				this._lastView.WriteXML(context);
			}
			context.writer.WriteEndElement();
		}
		public void AccumulateRobustHash(IRobustHash hash)
		{
			hash.Accumulate(this._displayName);
			this._latentRegionHolder.renderRegion.AccumulateRobustHash(hash);
		}
		internal LegendView GetLastView()
		{
			return this._lastView;
		}
		public void NotePositionUnlocked(LatLonZoom sourceMapPosition, MapPosition referenceMapPosition)
		{
			bool showingPreview = false;
			this._lastView = new LegendView(this, showingPreview, sourceMapPosition, referenceMapPosition);
		}
		public void NotePositionLocked(MapPosition referenceMapPosition)
		{
			D.Assert(false, "legend view never locked");
		}
		public string GetDisplayName()
		{
			return this.displayName;
		}
		public void SetDisplayName(string value)
		{
			this.displayName = value;
		}
		public IFuture GetRenderedLegendFuture(IDisplayableSource displayableSource, FutureFeatures features)
		{
			RenderRegion renderRegion = this.latentRegionHolder.renderRegion;
			if (renderRegion == null)
			{
				throw new Legend.RenderFailedException("Region unavailable");
			}
			renderRegion = renderRegion.Copy(new DirtyEvent());
			MapRectangleParameter mapRectangleParameter = new MapRectangleParameter(renderRegion.GetBoundingBox());
			Size outputSize = this.OutputSizeFromRenderRegion(renderRegion);
			IFuturePrototype imagePrototype = displayableSource.GetImagePrototype(new ImageParameterFromRawBounds(outputSize), features);
			return imagePrototype.Curry(new ParamDict(new object[]
			{
				TermName.ImageBounds,
				mapRectangleParameter
			}));
		}
		private Size OutputSizeFromRenderRegion(RenderRegion renderRegion)
		{
			MapRectangleParameter mapRectangleParameter = new MapRectangleParameter(renderRegion.GetBoundingBox());
			return mapRectangleParameter.value.SizeWithAspectRatio(this.renderedSize);
		}
		public ImageRef RenderLegend(IDisplayableSource displayableSource)
		{
			Present present = this.GetRenderedLegendFuture(displayableSource, FutureFeatures.Cached).Realize("Legend.RenderLegend");
			if (!(present is ImageRef))
			{
				throw new Legend.RenderFailedException("Render failed: " + present.ToString());
			}
			return (ImageRef)present;
		}
		internal Size GetOutputSizeSynchronously(IFuture synchronousUserBoundsFuture)
		{
			RenderRegion renderRegionSynchronously = this.latentRegionHolder.GetRenderRegionSynchronously(synchronousUserBoundsFuture);
			return this.OutputSizeFromRenderRegion(renderRegionSynchronously);
		}
	}
}
