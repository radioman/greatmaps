using MSR.CVE.BackMaker.ImagePipeline;
using System;
using System.IO;
using System.Text;
using System.Xml;
namespace MSR.CVE.BackMaker
{
	public class LayerMetadataFile
	{
		private const string filename = "LayerMetadata.xml";
		private RenderOutputMethod renderOutputMethod;
		private EncodableHash _encodableHash;
		private static string LayerMetadataTag = "LayerMetadata";
		public EncodableHash encodableHash
		{
			get
			{
				return this._encodableHash;
			}
		}
		public LayerMetadataFile(RenderOutputMethod renderOutputMethod, EncodableHash encodableHash)
		{
			this.renderOutputMethod = renderOutputMethod;
			this._encodableHash = encodableHash;
		}
		public static LayerMetadataFile Read(RenderOutputMethod outputMethod)
		{
			LayerMetadataFile layerMetadataFile = null;
			Stream input = outputMethod.ReadFile("LayerMetadata.xml");
			XmlTextReader reader = new XmlTextReader(input);
			MashupParseContext mashupParseContext = new MashupParseContext(reader);
			using (mashupParseContext)
			{
				while (mashupParseContext.reader.Read())
				{
					if (mashupParseContext.reader.NodeType == XmlNodeType.Element && mashupParseContext.reader.Name == LayerMetadataFile.LayerMetadataTag)
					{
						layerMetadataFile = new LayerMetadataFile(outputMethod, mashupParseContext);
						break;
					}
				}
				mashupParseContext.Dispose();
			}
			if (layerMetadataFile == null)
			{
				throw new InvalidMashupFile(mashupParseContext, string.Format("{0} doesn't appear to be a valid {1}", outputMethod.GetUri("LayerMetadata.xml"), LayerMetadataFile.LayerMetadataTag));
			}
			return layerMetadataFile;
		}
		private LayerMetadataFile(RenderOutputMethod renderOutputMethod, MashupParseContext context)
		{
			this.renderOutputMethod = renderOutputMethod;
			XMLTagReader xMLTagReader = context.NewTagReader(LayerMetadataFile.LayerMetadataTag);
			while (xMLTagReader.FindNextStartTag())
			{
				if (xMLTagReader.TagIs("StrongHash"))
				{
					context.AssertUnique(this._encodableHash);
					this._encodableHash = new EncodableHash(context);
				}
			}
			context.AssertPresent(this._encodableHash, "StrongHash");
		}
		public void Write()
		{
			XmlTextWriter xmlTextWriter = new XmlTextWriter(this.renderOutputMethod.CreateFile("LayerMetadata.xml", "text/xml"), Encoding.UTF8);
			using (xmlTextWriter)
			{
				xmlTextWriter.Formatting = Formatting.Indented;
				xmlTextWriter.WriteStartDocument(true);
				this.WriteXML(xmlTextWriter);
				xmlTextWriter.Close();
			}
		}
		private void WriteXML(XmlTextWriter writer)
		{
			writer.WriteStartElement(LayerMetadataFile.LayerMetadataTag);
			this._encodableHash.WriteXML(writer);
			writer.WriteEndElement();
		}
	}
}
