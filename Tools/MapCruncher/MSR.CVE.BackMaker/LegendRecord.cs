using System;
using System.Drawing;
using System.Xml;
namespace MSR.CVE.BackMaker
{
	public class LegendRecord
	{
		public const string LegendPathPrefix = "legends";
		private const string legendTag = "Legend";
		private const string legendURLAttr = "URL";
		private const string legendDisplayNameAttr = "DisplayName";
		private string url;
		internal string urlSuffix;
		private string displayName;
		internal Size imageDimensions;
		public LegendRecord(string urlPrefix, string urlSuffix, string displayName, Size imageDimensions)
		{
			this.url = string.Format("{0}/{1}", urlPrefix, urlSuffix);
			this.urlSuffix = urlSuffix;
			this.displayName = displayName;
			this.imageDimensions = imageDimensions;
		}
		public LegendRecord(MashupParseContext context)
		{
			XMLTagReader xMLTagReader = context.NewTagReader("Legend");
			this.url = context.GetRequiredAttribute("URL");
			this.displayName = context.GetRequiredAttribute("DisplayName");
			object obj = null;
			while (xMLTagReader.FindNextStartTag())
			{
				if (xMLTagReader.TagIs("Size"))
				{
					context.AssertUnique(obj);
					obj = new object();
					this.imageDimensions = XMLUtils.ReadSize(context);
				}
			}
			context.AssertPresent(obj, "Size");
		}
		public void WriteXML(XmlTextWriter writer)
		{
			writer.WriteStartElement("Legend");
			writer.WriteAttributeString("URL", this.url);
			writer.WriteAttributeString("DisplayName", this.displayName);
			XMLUtils.WriteSize(this.imageDimensions, writer);
			writer.WriteEndElement();
		}
		internal static string GetXMLTag()
		{
			return "Legend";
		}
	}
}
