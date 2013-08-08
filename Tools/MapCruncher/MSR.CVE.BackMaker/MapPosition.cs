using MSR.CVE.BackMaker.ImagePipeline;
using System;
using System.Globalization;
using System.Xml;
namespace MSR.CVE.BackMaker
{
	public class MapPosition
	{
		private const string MapPositionTag = "MapPosition";
		private const string lonAttr = "lon";
		private const string latAttr = "lat";
		private const string zoomAttr = "zoom";
		private const string styleAttr = "style";
		private LatLonZoom _llz;
		private string _style;
		private bool valid;
		private PositionUpdateIfc updateIfc;
		public LatLonZoom llz
		{
			get
			{
				return this._llz;
			}
		}
		public string style
		{
			get
			{
				return this._style;
			}
		}
		public MapPosition(PositionUpdateIfc updateIfc)
		{
			this.updateIfc = updateIfc;
		}
		public MapPosition(LatLonZoom llz, string style, PositionUpdateIfc updateIfc)
		{
			this._llz = llz;
			this._style = style;
			this.valid = true;
			this.updateIfc = updateIfc;
		}
		public MapPosition(MapPosition prototype, PositionUpdateIfc updateIfc)
		{
			this._llz = prototype._llz;
			this._style = prototype._style;
			this.valid = prototype.valid;
			this.updateIfc = updateIfc;
		}
		public override bool Equals(object o2)
		{
			if (o2 is MapPosition)
			{
				MapPosition mapPosition = (MapPosition)o2;
				return this._llz == mapPosition._llz;
			}
			return false;
		}
		public override string ToString()
		{
			return this._llz.ToString();
		}
		private void setBase()
		{
			if (this.updateIfc != null)
			{
				this.updateIfc.PositionUpdated(this._llz);
			}
			this.valid = true;
		}
		public void setPosition(LatLonZoom llz, string style)
		{
			this._llz = llz;
			this._style = style;
			this.setBase();
		}
		public void setPosition(LatLonZoom llz)
		{
			this._llz = llz;
			this.setBase();
		}
		public void setPosition(MapPosition p)
		{
			if (p.IsValid())
			{
				this.setPosition(p.llz, p.style);
			}
		}
		public void setZoom(int zoom)
		{
			this._llz = new LatLonZoom(this.llz.lat, this.llz.lon, zoom);
			this.setBase();
		}
		public void setStyle(string style)
		{
			this._style = style;
			this.setBase();
		}
		public bool IsValid()
		{
			return this.valid;
		}
		public override int GetHashCode()
		{
			return this._llz.GetHashCode() ^ this._style.GetHashCode();
		}
		public static string GetXMLTag(MashupXMLSchemaVersion version)
		{
			if (version == MonolithicMapPositionsSchema.schema)
			{
				return "MapPosition";
			}
			return "MapPosition";
		}
		public void WriteXML(XmlWriter writer)
		{
			writer.WriteStartElement("MapPosition");
			if (this.style != null)
			{
				writer.WriteStartAttribute("style");
				writer.WriteValue(this.style);
				writer.WriteEndAttribute();
			}
			this.llz.WriteXML(writer);
			writer.WriteEndElement();
		}
		public MapPosition(MashupParseContext context, PositionUpdateIfc updateIfc, CoordinateSystemIfc coordSys)
		{
			this.updateIfc = updateIfc;
			if (context.version == MonolithicMapPositionsSchema.schema)
			{
				XMLTagReader xMLTagReader = context.NewTagReader("MapPosition");
				this._llz = new LatLonZoom(Convert.ToDouble(context.reader.GetAttribute("lat"), CultureInfo.InvariantCulture), Convert.ToDouble(context.reader.GetAttribute("lon"), CultureInfo.InvariantCulture), Convert.ToInt32(context.reader.GetAttribute("zoom"), CultureInfo.InvariantCulture));
				if (context.reader.GetAttribute("style") != null)
				{
					this.setStyle(context.reader.GetAttribute("style"));
				}
				this.valid = true;
				while (xMLTagReader.FindNextStartTag())
				{
				}
				return;
			}
			XMLTagReader xMLTagReader2 = context.NewTagReader("MapPosition");
			if (context.reader.GetAttribute("style") != null)
			{
				this.setStyle(context.reader.GetAttribute("style"));
			}
			while (xMLTagReader2.FindNextStartTag())
			{
				if (xMLTagReader2.TagIs(LatLonZoom.GetXMLTag()))
				{
					this._llz = new LatLonZoom(context, coordSys);
					this.valid = true;
				}
			}
			while (xMLTagReader2.FindNextStartTag())
			{
			}
		}
		internal void ForceInteractiveUpdate()
		{
			if (this.updateIfc != null)
			{
				this.updateIfc.ForceInteractiveUpdate();
			}
		}
	}
}
