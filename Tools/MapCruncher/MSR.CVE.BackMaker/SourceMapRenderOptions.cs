using MSR.CVE.BackMaker.ImagePipeline;
using System;
using System.Globalization;
using System.Xml;
namespace MSR.CVE.BackMaker
{
	public class SourceMapRenderOptions : IRobustlyHashable
	{
		public const int UNDEFINED_ZOOM = -1;
		private const string SourceMapRenderOptionsTag = "SourceMapRenderOptions";
		private const string MinZoomAttr = "MinZoom";
		private const string MaxZoomAttr = "MaxZoom";
		private int _minZoom = 1;
		private int _maxZoom = -1;
		public DirtyEvent dirtyEvent;
		public int minZoom
		{
			get
			{
				return this._minZoom;
			}
			set
			{
				if (value != this._minZoom)
				{
					this._minZoom = value;
					this.dirtyEvent.SetDirty();
				}
			}
		}
		public int maxZoom
		{
			get
			{
				return this._maxZoom;
			}
			set
			{
				if (value != this._maxZoom)
				{
					this._maxZoom = value;
					this.dirtyEvent.SetDirty();
				}
			}
		}
		public SourceMapRenderOptions(DirtyEvent parentDirty)
		{
			this.dirtyEvent = new DirtyEvent(parentDirty);
		}
		public SourceMapRenderOptions(MashupParseContext context, DirtyEvent parentDirty)
		{
			this.dirtyEvent = new DirtyEvent(parentDirty);
			XMLTagReader xMLTagReader = context.NewTagReader("SourceMapRenderOptions");
			new MercatorCoordinateSystem();
			string attribute = context.reader.GetAttribute("MinZoom");
			if (attribute != null)
			{
				this._minZoom = MercatorCoordinateSystem.theInstance.GetZoomRange().Parse(context, "MinZoom", attribute);
			}
			else
			{
				this._minZoom = MercatorCoordinateSystem.theInstance.GetZoomRange().min;
			}
			this._maxZoom = MercatorCoordinateSystem.theInstance.GetZoomRange().ParseAllowUndefinedZoom(context, "MaxZoom", context.reader.GetAttribute("MaxZoom"));
			if (this._minZoom > this._maxZoom)
			{
				throw new InvalidMashupFile(context, string.Format("MinZoom {0} > MaxZoom {1}", this._minZoom, this._maxZoom));
			}
			xMLTagReader.SkipAllSubTags();
		}
		internal void WriteXML(XmlTextWriter writer)
		{
			writer.WriteStartElement("SourceMapRenderOptions");
			writer.WriteAttributeString("MinZoom", this._minZoom.ToString(CultureInfo.InvariantCulture));
			writer.WriteAttributeString("MaxZoom", this._maxZoom.ToString(CultureInfo.InvariantCulture));
			writer.WriteEndElement();
		}
		internal static string GetXMLTag()
		{
			return "SourceMapRenderOptions";
		}
		public override int GetHashCode()
		{
			return this.minZoom * 131 + this.maxZoom;
		}
		public override bool Equals(object obj)
		{
			return this.minZoom == ((SourceMapRenderOptions)obj).minZoom;
		}
		public void AccumulateRobustHash(IRobustHash hash)
		{
			hash.Accumulate(this.minZoom);
			hash.Accumulate(this.maxZoom);
		}
	}
}
