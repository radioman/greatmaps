using System;
using System.Xml;
namespace MSR.CVE.BackMaker
{
	public class SourceMapInfo
	{
		private const string SourceMapInfoTag = "SourceMapInfo";
		public const string SourceMapFileURLTag = "MapFileURL";
		public const string SourceMapHomePageTag = "MapHomePage";
		public const string SourceMapDescriptionTag = "MapDescription";
		public const string UrlAttr = "url";
		private DirtyEvent dirtyEvent;
		private string _mapFileURL = "";
		private string _mapHomePage = "";
		private string _mapDescription = "";
		public string mapFileURL
		{
			get
			{
				return this._mapFileURL;
			}
			set
			{
				if (this._mapFileURL != value)
				{
					this._mapFileURL = value;
					this.dirtyEvent.SetDirty();
				}
			}
		}
		public string mapHomePage
		{
			get
			{
				return this._mapHomePage;
			}
			set
			{
				if (this._mapHomePage != value)
				{
					this._mapHomePage = value;
					this.dirtyEvent.SetDirty();
				}
			}
		}
		public string mapDescription
		{
			get
			{
				return this._mapDescription;
			}
			set
			{
				if (this._mapDescription != value)
				{
					this._mapDescription = value;
					this.dirtyEvent.SetDirty();
				}
			}
		}
		public SourceMapInfo(DirtyEvent parentDirty)
		{
			this.dirtyEvent = new DirtyEvent(parentDirty);
		}
		public static string GetXMLTag()
		{
			return "SourceMapInfo";
		}
		public void WriteXML(XmlTextWriter writer)
		{
			writer.WriteStartElement("SourceMapInfo");
			writer.WriteStartElement("MapFileURL");
			writer.WriteAttributeString("url", this.mapFileURL);
			writer.WriteEndElement();
			writer.WriteStartElement("MapHomePage");
			writer.WriteAttributeString("url", this.mapHomePage);
			writer.WriteEndElement();
			XMLUtils.WriteStringXml(writer, "MapDescription", this.mapDescription);
			writer.WriteEndElement();
		}
		public SourceMapInfo(MashupParseContext context, DirtyEvent parentDirty)
		{
			this.dirtyEvent = new DirtyEvent(parentDirty);
			XMLTagReader xMLTagReader = context.NewTagReader(SourceMapInfo.GetXMLTag());
			while (xMLTagReader.FindNextStartTag())
			{
				if (xMLTagReader.TagIs("MapFileURL"))
				{
					if (context.version == SourceMapInfoAsCharDataSchema.schema)
					{
						this._mapFileURL = XMLUtils.ReadStringXml(context, "MapFileURL");
					}
					else
					{
						XMLTagReader xMLTagReader2 = context.NewTagReader("MapFileURL");
						this._mapFileURL = context.GetRequiredAttribute("url");
						xMLTagReader2.SkipAllSubTags();
					}
				}
				else
				{
					if (xMLTagReader.TagIs("MapHomePage"))
					{
						if (context.version == SourceMapInfoAsCharDataSchema.schema)
						{
							this._mapHomePage = XMLUtils.ReadStringXml(context, "MapHomePage");
						}
						else
						{
							XMLTagReader xMLTagReader3 = context.NewTagReader("MapHomePage");
							this._mapHomePage = context.GetRequiredAttribute("url");
							xMLTagReader3.SkipAllSubTags();
						}
					}
					else
					{
						if (xMLTagReader.TagIs("MapDescription"))
						{
							this._mapDescription = XMLUtils.ReadStringXml(context, "MapDescription");
						}
					}
				}
			}
		}
	}
}
