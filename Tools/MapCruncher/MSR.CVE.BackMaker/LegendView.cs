using MSR.CVE.BackMaker.ImagePipeline;
using System;
using System.Globalization;
namespace MSR.CVE.BackMaker
{
	public class LegendView : IMapView, ICurrentView
	{
		private Legend _legend;
		private bool _showingPreview;
		private LatLonZoom sourceMapView;
		private MapPosition referenceMapView;
		private static string previewAttr = "ShowingPreview";
		private static string sourceMapViewTag = "SourceMapPosition";
		private static string referenceMapViewTag = "ReferenceMapPosition";
		public Legend legend
		{
			get
			{
				return this._legend;
			}
		}
		public bool showingPreview
		{
			get
			{
				return this._showingPreview;
			}
		}
		public object GetViewedObject()
		{
			return this.legend;
		}
		public LegendView(Legend legend, bool showingPreview, LatLonZoom sourceMapView, MapPosition referenceMapView)
		{
			this._legend = legend;
			this._showingPreview = showingPreview;
			this.sourceMapView = sourceMapView;
			this.referenceMapView = referenceMapView;
		}
		public LegendView(Legend legend, MashupParseContext context)
		{
			this._legend = legend;
			object obj = null;
			XMLTagReader xMLTagReader = context.NewTagReader(LegendView.GetXMLTag());
			this._showingPreview = context.GetRequiredAttributeBoolean(LegendView.previewAttr);
			while (xMLTagReader.FindNextStartTag())
			{
				if (xMLTagReader.TagIs(LegendView.sourceMapViewTag))
				{
					XMLTagReader xMLTagReader2 = context.NewTagReader(LegendView.sourceMapViewTag);
					while (xMLTagReader2.FindNextStartTag())
					{
						if (xMLTagReader2.TagIs(LatLonZoom.GetXMLTag()))
						{
							this.sourceMapView = new LatLonZoom(context, ContinuousCoordinateSystem.theInstance);
							context.AssertUnique(obj);
							obj = new object();
						}
					}
				}
				else
				{
					if (xMLTagReader.TagIs(LegendView.referenceMapViewTag))
					{
						context.AssertUnique(this.referenceMapView);
						XMLTagReader xMLTagReader3 = context.NewTagReader(LegendView.referenceMapViewTag);
						while (xMLTagReader3.FindNextStartTag())
						{
							if (xMLTagReader3.TagIs(MapPosition.GetXMLTag(context.version)))
							{
								this.referenceMapView = new MapPosition(context, null, MercatorCoordinateSystem.theInstance);
							}
						}
					}
				}
			}
			context.AssertPresent(obj, LegendView.sourceMapViewTag);
			context.AssertPresent(this.referenceMapView, LegendView.referenceMapViewTag);
		}
		public static string GetXMLTag()
		{
			return "LegendView";
		}
		public void WriteXML(MashupWriteContext wc)
		{
			wc.writer.WriteStartElement(LegendView.GetXMLTag());
			wc.writer.WriteAttributeString(LegendView.previewAttr, this._showingPreview.ToString(CultureInfo.InvariantCulture));
			wc.writer.WriteStartElement(LegendView.sourceMapViewTag);
			this.sourceMapView.WriteXML(wc.writer);
			wc.writer.WriteEndElement();
			wc.writer.WriteStartElement(LegendView.referenceMapViewTag);
			this.referenceMapView.WriteXML(wc.writer);
			wc.writer.WriteEndElement();
			wc.writer.WriteEndElement();
		}
		public MapPosition GetReferenceMapView()
		{
			return this.referenceMapView;
		}
		public LatLonZoom GetSourceMapView()
		{
			return this.sourceMapView;
		}
	}
}
