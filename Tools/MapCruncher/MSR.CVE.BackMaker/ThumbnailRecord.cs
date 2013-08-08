using System;
using System.Drawing;
using System.Globalization;
using System.Xml;
namespace MSR.CVE.BackMaker
{
	public class ThumbnailRecord
	{
		private const string ThumbnailTag = "Thumbnail";
		private const string UrlAttr = "URL";
		private const string WidthAttr = "Width";
		private const string HeightAttr = "Height";
		internal string urlRelativePath;
		internal Size size;
		public ThumbnailRecord(string urlRelativePath, Size size)
		{
			this.urlRelativePath = urlRelativePath;
			this.size = size;
		}
		internal void WriteXML(XmlTextWriter writer)
		{
			writer.WriteStartElement("Thumbnail");
			writer.WriteAttributeString("URL", this.urlRelativePath);
			writer.WriteAttributeString("Width", this.size.Width.ToString(CultureInfo.InvariantCulture));
			writer.WriteAttributeString("Height", this.size.Height.ToString(CultureInfo.InvariantCulture));
			writer.WriteEndElement();
		}
		internal string WriteImgTag(string pathPrefix)
		{
			return string.Format("<img src=\"{3}{0}\" width=\"{1}\" height=\"{2}\">", new object[]
			{
				this.urlRelativePath,
				this.size.Width,
				this.size.Height,
				pathPrefix
			});
		}
	}
}
