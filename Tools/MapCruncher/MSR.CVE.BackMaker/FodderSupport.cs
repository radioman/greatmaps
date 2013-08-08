using System;
using System.Text;
using System.Xml;
namespace MSR.CVE.BackMaker
{
	internal class FodderSupport
	{
		public static string CanonicalMercatorQuadTreeString = "MercatorQuadTree";
		public static string MapCruncherAppIDString = "MSRMapCruncher";
		public static string searchFodderTag = "SearchFodder";
		public static string DigitsToLetters(string digitalInput)
		{
			string text = "";
			for (int i = 0; i < digitalInput.Length; i++)
			{
				text += Convert.ToChar((int)('A' + (digitalInput[i] - '0')));
			}
			return text;
		}
		public static void WriteAppFodderString(XmlTextWriter writer, string appName, string s)
		{
			writer.WriteStartElement(FodderSupport.searchFodderTag);
			writer.WriteString(FodderSupport.CanonicalMercatorQuadTreeString + s);
			writer.WriteEndElement();
		}
		public static void WriteQuadTreeFodderString(XmlTextWriter writer, string s)
		{
			writer.WriteStartElement(FodderSupport.searchFodderTag);
			writer.WriteString(FodderSupport.CanonicalMercatorQuadTreeString + s);
			writer.WriteEndElement();
		}
		public static string ExtractDigits(string input)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < input.Length; i++)
			{
				char c = input[i];
				if (c >= '0' && c <= '9')
				{
					stringBuilder.Append(c);
				}
			}
			return stringBuilder.ToString();
		}
	}
}
