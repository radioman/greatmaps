using MSR.CVE.BackMaker.ImagePipeline;
using System;
using System.Globalization;
using System.Xml;
namespace MSR.CVE.BackMaker
{
	public struct LatLonZoom : IRobustlyHashable
	{
		private const string LatLonZoomTag = "LatLonZoom";
		private const string zoomAttr = "zoom";
		private LatLon _latlon;
		private int _zoom;
		public LatLon latlon
		{
			get
			{
				return this._latlon;
			}
		}
		public int zoom
		{
			get
			{
				return this._zoom;
			}
		}
		public double lat
		{
			get
			{
				return this._latlon.lat;
			}
		}
		public double lon
		{
			get
			{
				return this._latlon.lon;
			}
		}
		public LatLonZoom(double lat, double lon, int zoom)
		{
			this._latlon = new LatLon(lat, lon);
			this._zoom = zoom;
		}
		public LatLonZoom(LatLon latlon, int zoom)
		{
			this._latlon = latlon;
			this._zoom = zoom;
		}
		public override bool Equals(object o)
		{
			if (o is LatLonZoom)
			{
				LatLonZoom llz = (LatLonZoom)o;
				return this == llz;
			}
			return false;
		}
		public override int GetHashCode()
		{
			return this._latlon.GetHashCode() ^ this._zoom.GetHashCode();
		}
		public static bool operator ==(LatLonZoom llz1, LatLonZoom llz2)
		{
			return llz1._latlon == llz2._latlon && llz1._zoom == llz2._zoom;
		}
		public static bool operator !=(LatLonZoom llz1, LatLonZoom llz2)
		{
			return llz1._latlon != llz2._latlon || llz1._zoom != llz2._zoom;
		}
		public override string ToString()
		{
			return string.Format("{0} {1}Z", this._latlon, this._zoom);
		}
		public static string GetXMLTag()
		{
			return "LatLonZoom";
		}
		public void WriteXML(XmlWriter writer)
		{
			writer.WriteStartElement("LatLonZoom");
			writer.WriteAttributeString("zoom", this._zoom.ToString(CultureInfo.InvariantCulture));
			this._latlon.WriteXML(writer);
			writer.WriteEndElement();
		}
		public void WriteXMLToAttributes(XmlWriter writer)
		{
			this._latlon.WriteXMLToAttributes(writer);
			writer.WriteAttributeString("zoom", this._zoom.ToString(CultureInfo.InvariantCulture));
		}
		public LatLonZoom(MashupParseContext context, CoordinateSystemIfc coordSys)
		{
			XMLTagReader xMLTagReader = context.NewTagReader("LatLonZoom");
			try
			{
				if (context.reader.GetAttribute("zoom") == null)
				{
					throw new InvalidLLZ(context, "Missing zoom attribute");
				}
				try
				{
					this._zoom = coordSys.GetZoomRange().ParseAllowUndefinedZoom(context, "zoom", context.reader.GetAttribute("zoom"));
				}
				catch (InvalidMashupFile invalidMashupFile)
				{
					throw new InvalidLLZ(context, invalidMashupFile.Message);
				}
				bool flag = false;
				this._latlon = default(LatLon);
				while (xMLTagReader.FindNextStartTag())
				{
					if (xMLTagReader.TagIs(LatLon.GetXMLTag()))
					{
						this._latlon = new LatLon(context, coordSys);
						flag = true;
					}
				}
				if (!flag)
				{
					throw new InvalidLLZ(context, "Missing LatLong Tag");
				}
			}
			finally
			{
				xMLTagReader.SkipAllSubTags();
			}
		}
		public static LatLonZoom ReadFromAttributes(MashupParseContext context, CoordinateSystemIfc coordSys)
		{
			int zoom = coordSys.GetZoomRange().ParseAllowUndefinedZoom(context, "zoom", context.GetRequiredAttribute("zoom"));
			LatLon latLon = LatLon.ReadFromAttributes(context, coordSys);
			return new LatLonZoom(latLon.lat, latLon.lon, zoom);
		}
		public static LatLonZoom USA()
		{
			return new LatLonZoom(40.0, -96.0, 3);
		}
		public static LatLonZoom World()
		{
			return new LatLonZoom(0.0, 0.0, 1);
		}
		public void AccumulateRobustHash(IRobustHash hash)
		{
			this.latlon.AccumulateRobustHash(hash);
			hash.Accumulate(this.zoom);
		}
	}
}
