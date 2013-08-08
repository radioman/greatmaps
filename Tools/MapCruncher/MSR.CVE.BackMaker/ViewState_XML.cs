using System;
using System.Xml;
namespace MSR.CVE.BackMaker
{
	public class ViewState_XML
	{
		private const string XMLElementName = "ViewState";
		private const string XMLAttributeName = "Value";
		private const string XMLAttributeValue_Slaved = "Locked";
		private const string XMLAttributeValue_Unslaved = "Unlocked";
		public static string GetXMLTag()
		{
			return "ViewState";
		}
		public static void WriteXML(ViewState viewState, XmlTextWriter writer)
		{
			string value;
			if (viewState == ViewState.Slaved)
			{
				value = "Locked";
			}
			else
			{
				D.Assert(viewState == ViewState.Unslaved);
				value = "Unlocked";
			}
			writer.WriteStartElement("ViewState");
			writer.WriteAttributeString("Value", value);
			writer.WriteEndElement();
		}
		public static ViewState ReadXML(MashupParseContext context)
		{
			string requiredAttribute = context.GetRequiredAttribute("Value");
			if (requiredAttribute == "Locked")
			{
				return ViewState.Slaved;
			}
			D.Assert(requiredAttribute == "Unlocked");
			return ViewState.Unslaved;
		}
	}
}
