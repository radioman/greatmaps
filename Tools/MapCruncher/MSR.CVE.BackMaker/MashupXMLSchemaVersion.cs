using System;
using System.Collections.Generic;
using System.Xml;
namespace MSR.CVE.BackMaker
{
	public class MashupXMLSchemaVersion
	{
		private const string mashupFileVersionNumberTag = "Version";
		public const string llLatAttr = "lat";
		public const string llLonAttr = "lon";
		public const string llzZoomAttr = "zoom";
		public const string mapPositionTag = "MapPosition";
		public const string mpStyleAttr = "style";
		private string _versionNumberString;
		private static List<MashupXMLSchemaVersion> _AcceptedVersions;
		public string versionNumberString
		{
			get
			{
				return this._versionNumberString;
			}
		}
		public static List<MashupXMLSchemaVersion> AcceptedVersions
		{
			get
			{
				return MashupXMLSchemaVersion._AcceptedVersions;
			}
		}
		protected MashupXMLSchemaVersion(string versionNumberString)
		{
			this._versionNumberString = versionNumberString;
		}
		public void WriteXMLAttribute(XmlTextWriter writer)
		{
			writer.WriteAttributeString("Version", this.versionNumberString);
		}
		public static MashupXMLSchemaVersion ReadXMLAttribute(XmlTextReader reader)
		{
			string versionString = reader.GetAttribute("Version");
			MashupXMLSchemaVersion mashupXMLSchemaVersion = MashupXMLSchemaVersion.AcceptedVersions.Find((MashupXMLSchemaVersion vi) => vi._versionNumberString == versionString);
			if (mashupXMLSchemaVersion == null)
			{
				throw new InvalidMashupFile(reader, string.Format("Unknown mashup file version {0}", versionString));
			}
			return mashupXMLSchemaVersion;
		}
		static MashupXMLSchemaVersion()
		{
			MashupXMLSchemaVersion._AcceptedVersions = new List<MashupXMLSchemaVersion>();
			MashupXMLSchemaVersion._AcceptedVersions.Add(CurrentSchema.schema);
			MashupXMLSchemaVersion._AcceptedVersions.Add(NoTagIdentities.schema);
			MashupXMLSchemaVersion._AcceptedVersions.Add(ViewsNotAsWellPreservedSchema.schema);
			MashupXMLSchemaVersion._AcceptedVersions.Add(SingleMaxZoomForEntireMashupSchema.schema);
			MashupXMLSchemaVersion._AcceptedVersions.Add(SourceMapInfoAsCharDataSchema.schema);
			MashupXMLSchemaVersion._AcceptedVersions.Add(InlineSourceMapInfoSchema.schema);
			MashupXMLSchemaVersion._AcceptedVersions.Add(MonolithicMapPositionsSchema.schema);
		}
	}
}
