using MSR.CVE.BackMaker.ImagePipeline;
using System;
using System.Xml;
namespace MSR.CVE.BackMaker
{
	public class LayerView : IMapView, ICurrentView
	{
		private Layer _layer;
		private MapPosition lockedView;
		public Layer layer
		{
			get
			{
				return this._layer;
			}
		}
		public object GetViewedObject()
		{
			return this.layer;
		}
		public LayerView(Layer layer, MapPosition lockedView)
		{
			this._layer = layer;
			this.lockedView = lockedView;
		}
		public static string GetXMLTag()
		{
			return "LayerView";
		}
		public LayerView(Layer layer, MashupParseContext context)
		{
			this._layer = layer;
			bool flag = false;
			XMLTagReader xMLTagReader = context.NewTagReader(LayerView.GetXMLTag());
			while (xMLTagReader.FindNextStartTag())
			{
				if (xMLTagReader.TagIs(MapPosition.GetXMLTag(context.version)))
				{
					this.lockedView = new MapPosition(context, null, MercatorCoordinateSystem.theInstance);
					flag = true;
				}
			}
			if (!flag)
			{
				throw new InvalidMashupFile(context, "No LatLonZoom tag in LayerView.");
			}
		}
		public void WriteXML(XmlTextWriter writer)
		{
			writer.WriteStartElement(LayerView.GetXMLTag());
			this.lockedView.WriteXML(writer);
			writer.WriteEndElement();
		}
		public MapPosition GetReferenceMapView()
		{
			return this.lockedView;
		}
		public LatLonZoom GetSourceMapView()
		{
			return this.lockedView.llz;
		}
	}
}
