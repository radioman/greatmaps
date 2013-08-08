using MSR.CVE.BackMaker.ImagePipeline;
using System;
using System.Xml;
namespace MSR.CVE.BackMaker
{
	public abstract class RenderedTileNamingScheme
	{
		private const string TileNamingSchemeTag = "TileNamingScheme";
		private const string NamingSchemeTypeAttr = "Type";
		private const string FilePathAttr = "FilePath";
		private const string FilePrefixAttr = "FilePrefix";
		private const string FileSuffixAttr = "FileSuffix";
		protected string filePrefix;
		protected string fileSuffix;
		protected RenderedTileNamingScheme(string filePrefix, string fileSuffix)
		{
			this.filePrefix = filePrefix;
			D.Assert(fileSuffix.StartsWith("."));
			this.fileSuffix = fileSuffix;
		}
		public abstract string GetSchemeName();
		public abstract string GetTileFilename(TileAddress ta);
		public string GetRenderPath(TileAddress ta)
		{
			return this.GetFilePrefix() + "\\" + this.GetTileFilename(ta);
		}
		public static string GetXMLTag()
		{
			return "TileNamingScheme";
		}
		public string GetSchemeTag()
		{
			return "Type";
		}
		internal string GetFilePrefix()
		{
			return this.filePrefix;
		}
		public string GetFileSuffix()
		{
			return this.fileSuffix;
		}
		public void WriteXML(XmlTextWriter writer)
		{
			writer.WriteStartElement("TileNamingScheme");
			writer.WriteAttributeString(this.GetSchemeTag(), this.GetSchemeName());
			writer.WriteAttributeString("FilePath", "");
			writer.WriteAttributeString("FilePrefix", this.filePrefix);
			writer.WriteAttributeString("FileSuffix", this.fileSuffix);
			writer.WriteEndElement();
		}
		public static RenderedTileNamingScheme ReadXML(MashupParseContext context)
		{
			XMLTagReader xMLTagReader = context.NewTagReader("TileNamingScheme");
			string attribute = context.reader.GetAttribute("Type");
			string attribute2 = context.reader.GetAttribute("FilePrefix");
			string attribute3 = context.reader.GetAttribute("FileSuffix");
			xMLTagReader.SkipAllSubTags();
			if (attribute == null || attribute2 == null || attribute3 == null)
			{
				throw new InvalidMashupFile(context, string.Format("Invalid contents in {0} tag.", "TileNamingScheme"));
			}
			if (attribute == VENamingScheme.SchemeName)
			{
				return new VENamingScheme(attribute2, attribute3);
			}
			throw new InvalidMashupFile(context, "Unknown type: " + attribute);
		}
		internal void AccumulateRobustHash(IRobustHash hash)
		{
			hash.Accumulate("RenderedTileNamingScheme(");
			hash.Accumulate(this.GetSchemeName());
			hash.Accumulate(",");
			hash.Accumulate(this.filePrefix);
			hash.Accumulate(",");
			hash.Accumulate(this.fileSuffix);
			hash.Accumulate(")");
		}
	}
}
