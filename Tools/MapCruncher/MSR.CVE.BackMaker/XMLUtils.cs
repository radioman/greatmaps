using System;
using System.Drawing;
using System.Globalization;
using System.Xml;
namespace MSR.CVE.BackMaker
{
	public class XMLUtils
	{
		public const string sizeTag = "Size";
		private const string widthAttr = "Width";
		private const string heightAttr = "Height";
		public static Size ReadSize(MashupParseContext context)
		{
			XMLTagReader xMLTagReader = context.NewTagReader("Size");
			Size result = new Size(context.GetRequiredAttributeInt("Width"), context.GetRequiredAttributeInt("Height"));
			xMLTagReader.SkipAllSubTags();
			return result;
		}
		public static void WriteSize(Size size, XmlTextWriter writer)
		{
			writer.WriteStartElement("Size");
			writer.WriteAttributeString("Width", size.Width.ToString(CultureInfo.InvariantCulture));
			writer.WriteAttributeString("Height", size.Height.ToString(CultureInfo.InvariantCulture));
			writer.WriteEndElement();
		}
		public static void WriteStringXml(XmlTextWriter writer, string TagName, string value)
		{
			writer.WriteStartElement(TagName);
			writer.WriteString(value);
			writer.WriteEndElement();
		}
		public static string ReadStringXml(MashupParseContext context, string TagName)
		{
			XMLTagReader xMLTagReader = context.NewTagReader(TagName);
			xMLTagReader.SkipAllSubTags();
			return xMLTagReader.GetContent();
		}
	}
}
