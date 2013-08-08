using MSR.CVE.BackMaker.ImagePipeline;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Xml;
namespace MSR.CVE.BackMaker
{
	public class CrunchedLayer : ThumbnailCollection
	{
		public const string ReferenceNameAttr = "ReferenceName";
		public const string defaultViewTag = "DefaultView";
		public const string RangeDescriptorsTag = "RangeDescriptors";
		public const string SourceMapInfoListTag_compat = "SourceMapInfoList";
		public const string SourceMapRecordListTag = "SourceMapRecordList";
		public string displayName;
		private List<RangeDescriptor> rangeDescriptors;
		private List<SourceMapRecord> sourceMapRecords = new List<SourceMapRecord>();
		private List<ThumbnailRecord> thumbnailRecords = new List<ThumbnailRecord>();
		public LatLonZoom defaultView;
		public RenderedTileNamingScheme namingScheme;
		public SourceMapRecord this[SourceMap sourceMap]
		{
			get
			{
				int num = this.sourceMapRecords.FindIndex((SourceMapRecord smr) => smr.displayName == sourceMap.displayName);
				if (num == -1)
				{
					throw new IndexOutOfRangeException();
				}
				return this.sourceMapRecords[num];
			}
		}
		public static string GetXMLTag()
		{
			return Layer.GetXMLTag();
		}
		public CrunchedLayer(RenderOptions renderOptions, Layer layer, List<RangeDescriptor> rangeDescriptors, MapTileSourceFactory mapTileSourceFactory)
		{
			this.displayName = layer.GetDisplayName();
			this.namingScheme = new VENamingScheme(layer.GetFilesystemName(), renderOptions.GetOutputTileSuffix());
			this.rangeDescriptors = rangeDescriptors;
			bool flag;
			this.defaultView = this.GetDefaultView(layer, new Size(600, 600), mapTileSourceFactory, out flag);
			foreach (SourceMap current in layer)
			{
				this.sourceMapRecords.Add(new SourceMapRecord(layer, current, mapTileSourceFactory));
			}
		}
		public CrunchedLayer(MashupParseContext context)
		{
			this.displayName = context.reader.GetAttribute(Layer.GetLayerDisplayNameTag());
			XMLTagReader xMLTagReader = context.NewTagReader(Layer.GetXMLTag());
			while (xMLTagReader.FindNextStartTag())
			{
				if (xMLTagReader.TagIs("RangeDescriptors"))
				{
					XMLTagReader xMLTagReader2 = context.NewTagReader("RangeDescriptors");
					xMLTagReader2.SkipAllSubTags();
				}
				else
				{
					if (xMLTagReader.TagIs("DefaultView"))
					{
						XMLTagReader xMLTagReader3 = context.NewTagReader("DefaultView");
						this.defaultView = LatLonZoom.ReadFromAttributes(context, MercatorCoordinateSystem.theInstance);
						xMLTagReader3.SkipAllSubTags();
					}
					else
					{
						if (xMLTagReader.TagIs("SourceMapInfoList"))
						{
							XMLTagReader xMLTagReader4 = context.NewTagReader("SourceMapInfoList");
							while (xMLTagReader4.FindNextStartTag())
							{
								if (xMLTagReader4.TagIs(SourceMapInfo.GetXMLTag()))
								{
									this.sourceMapRecords.Add(new SourceMapRecord(new SourceMapInfo(context, new DirtyEvent())));
								}
							}
						}
						else
						{
							if (xMLTagReader.TagIs("SourceMapRecordList"))
							{
								XMLTagReader xMLTagReader5 = context.NewTagReader("SourceMapRecordList");
								while (xMLTagReader5.FindNextStartTag())
								{
									if (xMLTagReader5.TagIs(SourceMapRecord.GetXMLTag()))
									{
										this.sourceMapRecords.Add(new SourceMapRecord(context));
									}
								}
							}
							else
							{
								if (xMLTagReader.TagIs(RenderedTileNamingScheme.GetXMLTag()))
								{
									this.namingScheme = RenderedTileNamingScheme.ReadXML(context);
								}
							}
						}
					}
				}
			}
		}
		public void WriteXML(XmlTextWriter writer)
		{
			writer.WriteStartElement(CrunchedLayer.GetXMLTag());
			writer.WriteAttributeString(Layer.GetLayerDisplayNameTag(), this.displayName);
			writer.WriteAttributeString("ReferenceName", SampleHTMLWriter.ReferenceName(this.displayName));
			this.namingScheme.WriteXML(writer);
			writer.WriteStartElement("DefaultView");
			this.defaultView.WriteXMLToAttributes(writer);
			writer.WriteEndElement();
			writer.WriteStartElement("SourceMapRecordList");
			foreach (SourceMapRecord current in this.sourceMapRecords)
			{
				current.WriteXML(writer);
			}
			writer.WriteEndElement();
			writer.WriteStartElement("RangeDescriptors");
			foreach (RangeDescriptor current2 in this.rangeDescriptors)
			{
				current2.WriteXML(writer);
			}
			writer.WriteEndElement();
			foreach (ThumbnailRecord current3 in this.thumbnailRecords)
			{
				current3.WriteXML(writer);
			}
			writer.WriteEndElement();
		}
		internal LatLonZoom GetDefaultView(Layer layer, Size assumedDisplaySize, MapTileSourceFactory mapTileSourceFactory, out bool allBoundsValid)
		{
			MapRectangle mapRectangle = null;
			allBoundsValid = true;
			foreach (SourceMap current in layer)
			{
				if (!this.AccumulateBoundingBox(current, mapTileSourceFactory, ref mapRectangle))
				{
					allBoundsValid = false;
				}
			}
			LatLonZoom bestViewContaining;
			if (mapRectangle == null)
			{
				bestViewContaining = MercatorCoordinateSystem.theInstance.GetDefaultView();
			}
			else
			{
				bestViewContaining = MercatorCoordinateSystem.theInstance.GetBestViewContaining(mapRectangle, assumedDisplaySize);
			}
			return CoordinateSystemUtilities.ConstrainLLZ(MercatorCoordinateSystem.theInstance, bestViewContaining);
		}
		internal bool AccumulateBoundingBox(SourceMap sourceMap, MapTileSourceFactory mapTileSourceFactory, ref MapRectangle boundingBox)
		{
			bool result;
			try
			{
				WarpedMapTileSource warpedMapTileSource = mapTileSourceFactory.CreateWarpedSource(sourceMap);
				BoundsPresent boundsPresent = (BoundsPresent)warpedMapTileSource.GetUserBounds(null, FutureFeatures.Cached).Realize("CrunchedFile.AccumulateBoundingBox");
				boundsPresent.GetRenderRegion().AccumulateBoundingBox(ref boundingBox);
				result = true;
			}
			catch (InsufficientCorrespondencesException)
			{
				result = false;
			}
			return result;
		}
		internal void WriteSourceMapLegendFrames(RenderOutputMethod renderOutput)
		{
			foreach (SourceMapRecord current in this.sourceMapRecords)
			{
				current.WriteSourceMapLegendFrame(renderOutput);
			}
		}
		public void Add(ThumbnailRecord thumbnailRecord)
		{
			this.thumbnailRecords.Add(thumbnailRecord);
		}
	}
}
