using MSR.CVE.BackMaker.ImagePipeline;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using System.Xml;
namespace MSR.CVE.BackMaker
{
	public class SourceMapRecord : ThumbnailCollection
	{
		public const string ReferenceNameAttr = "ReferenceName";
		public const string sourceMapRecordTag = "SourceMapRecord";
		public const string displayNameAttr = "DisplayName";
		public const string maxZoomAttr = "MaxZoom";
		internal string displayName;
		private SourceMapLegendFrame sourceMapLegendFrame;
		private SourceMapInfo sourceMapInfo;
		private MapRectangle userBoundingRect;
		private IImageTransformer imageTransformer;
		private int maxZoom;
		private List<LegendRecord> legendRecords = new List<LegendRecord>();
		private List<ThumbnailRecord> thumbnailRecords = new List<ThumbnailRecord>();
		
        //[CompilerGenerated]
        //private static Comparison<ThumbnailRecord> <>9__CachedAnonymousMethodDelegate1;
		
        public SourceMapRecord(Layer layer, SourceMap sourceMap, MapTileSourceFactory mapTileSourceFactory)
		{
			this.displayName = sourceMap.displayName;
			this.sourceMapInfo = sourceMap.sourceMapInfo;
			this.userBoundingRect = sourceMap.GetUserBoundingBox(mapTileSourceFactory);
			this.maxZoom = sourceMap.sourceMapRenderOptions.maxZoom;
			try
			{
				this.imageTransformer = sourceMap.registration.warpStyle.getImageTransformer(sourceMap.registration, InterpolationMode.Invalid);
			}
			catch (Exception)
			{
			}
			foreach (Legend current in sourceMap.legendList)
			{
				this.legendRecords.Add(new LegendRecord("legends", sourceMap.GetLegendFilename(current), current.displayName, current.GetOutputSizeSynchronously(mapTileSourceFactory.CreateDisplayableUnwarpedSource(sourceMap).GetUserBounds(current.latentRegionHolder, FutureFeatures.Cached))));
			}
			this.sourceMapLegendFrame = new SourceMapLegendFrame(layer, sourceMap, this.legendRecords, new SourceMapLegendFrame.ThumbnailDelegate(this.thumbnailForLegendFrame));
		}
		public SourceMapRecord(SourceMapInfo sourceMapInfo)
		{
			this.displayName = "";
			this.sourceMapInfo = sourceMapInfo;
			this.userBoundingRect = null;
		}
		public SourceMapRecord(MashupParseContext context)
		{
			this.displayName = context.GetRequiredAttribute("DisplayName");
			XMLTagReader xMLTagReader = context.NewTagReader("SourceMapRecord");
			while (xMLTagReader.FindNextStartTag())
			{
				if (xMLTagReader.TagIs(SourceMapInfo.GetXMLTag()))
				{
					this.sourceMapInfo = new SourceMapInfo(context, new DirtyEvent());
				}
				else
				{
					if (xMLTagReader.TagIs(MapRectangle.GetXMLTag()))
					{
						this.userBoundingRect = new MapRectangle(context, MercatorCoordinateSystem.theInstance);
					}
					else
					{
						if (xMLTagReader.TagIs(LegendRecord.GetXMLTag()))
						{
							this.legendRecords.Add(new LegendRecord(context));
						}
						else
						{
							if (xMLTagReader.TagIs(SourceMapLegendFrame.GetXMLTag()))
							{
								context.AssertUnique(this.sourceMapLegendFrame);
								this.sourceMapLegendFrame = new SourceMapLegendFrame(context);
							}
						}
					}
				}
			}
		}
		public static string GetXMLTag()
		{
			return "SourceMapRecord";
		}
		public void WriteXML(XmlTextWriter writer)
		{
			writer.WriteStartElement(SourceMapRecord.GetXMLTag());
			writer.WriteAttributeString("DisplayName", this.displayName);
			writer.WriteAttributeString("ReferenceName", SampleHTMLWriter.ReferenceName(this.displayName));
			writer.WriteAttributeString("MaxZoom", this.maxZoom.ToString());
			this.sourceMapLegendFrame.WriteXML(writer);
			this.sourceMapInfo.WriteXML(writer);
			if (this.userBoundingRect != null)
			{
				this.userBoundingRect.WriteXML(writer);
			}
			foreach (LegendRecord current in this.legendRecords)
			{
				current.WriteXML(writer);
			}
			if (this.imageTransformer != null)
			{
				this.imageTransformer.writeToXml(writer);
			}
			foreach (ThumbnailRecord current2 in this.thumbnailRecords)
			{
				current2.WriteXML(writer);
			}
			writer.WriteEndElement();
		}
		public void WriteSourceMapLegendFrame(RenderOutputMethod renderOutput)
		{
			this.sourceMapLegendFrame.WriteSourceMapLegendFrame(renderOutput);
		}
		private ThumbnailRecord thumbnailForLegendFrame()
		{
			if (this.thumbnailRecords.Count == 0)
			{
				return null;
			}
			ThumbnailRecord[] array = this.thumbnailRecords.ToArray();
			Array.Sort<ThumbnailRecord>(array, (ThumbnailRecord r0, ThumbnailRecord r1) => Math.Abs(Math.Max(r0.size.Width, r0.size.Height) - 200) - Math.Abs(Math.Max(r1.size.Width, r1.size.Height) - 200));
			return array[0];
		}
		public void Add(ThumbnailRecord thumbnailRecord)
		{
			this.thumbnailRecords.Add(thumbnailRecord);
		}
	}
}
