using MSR.CVE.BackMaker.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Xml;
namespace MSR.CVE.BackMaker
{
	internal class CrunchedFile
	{
		private const string CrunchedFileVersionAttr = "Version";
		private const string CrunchedFileVersion = "1.6";
		private const string CrunchedFileIdentifierTag = "CrunchedFileIdentifier";
		private const string MapCruncherAppVersionTag = "MapCruncherAppVersion";
		private const string BoundsListTag = "BoundsList";
		private const string BoundsTag = "Bounds";
		private const string MapCruncherAppVersionAttr = "version";
		public List<CrunchedLayer> crunchedLayers = new List<CrunchedLayer>();
		public RenderOutputMethod renderOutput;
		public string sourceMashupFilename;
		public bool permitComposition;
		public List<TileRectangle> boundsList;
		public static string CrunchedFilename = "MapCruncherMetadata.xml";
		public static string CrunchedFilenameExtension = ".xml";
		public static string crunchedFileTag = "CrunchUp";
		public static string renderDateTag = "RenderDate";
		public static string SourceMashupFilenameAttr = "SourceMashupFilename";
		public CrunchedLayer this[Layer layer]
		{
			get
			{
				int num = this.crunchedLayers.FindIndex((CrunchedLayer cl) => cl.displayName == layer.displayName);
				if (num == -1)
				{
					throw new IndexOutOfRangeException();
				}
				return this.crunchedLayers[num];
			}
		}
		public CrunchedFile(Mashup mashup, RangeQueryData rangeQueryData, RenderOutputMethod renderOutput, string sourceMashupFilename, List<TileRectangle> boundsList, MapTileSourceFactory mapTileSourceFactory)
		{
			foreach (Layer current in mashup.layerList)
			{
				this.crunchedLayers.Add(new CrunchedLayer(mashup.GetRenderOptions(), current, rangeQueryData[current], mapTileSourceFactory));
			}
			this.renderOutput = renderOutput;
			this.sourceMashupFilename = sourceMashupFilename;
			this.permitComposition = mashup.GetRenderOptions().permitComposition;
			this.boundsList = boundsList;
		}
		public CrunchedFile(MashupParseContext context)
		{
			XMLTagReader xMLTagReader = context.NewTagReader(CrunchedFile.crunchedFileTag);
			while (xMLTagReader.FindNextStartTag())
			{
				if (xMLTagReader.TagIs(LayerList.GetXMLTag()))
				{
					XMLTagReader xMLTagReader2 = context.NewTagReader(LayerList.GetXMLTag());
					while (xMLTagReader2.FindNextStartTag())
					{
						if (xMLTagReader2.TagIs(CrunchedLayer.GetXMLTag()))
						{
							this.crunchedLayers.Add(new CrunchedLayer(context));
						}
					}
				}
			}
		}
		public static CrunchedFile FromUri(Uri uri)
		{
			XmlTextReader reader;
			if (uri.IsFile)
			{
				string localPath = uri.LocalPath;
				reader = new XmlTextReader(File.Open(localPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
			}
			else
			{
				if (!(uri.Scheme == "http"))
				{
					throw new Exception(string.Format("Unhandled URI scheme {0}", uri.Scheme));
				}
				HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
				httpWebRequest.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.Revalidate);
				D.Sayf(1, "Fetching {0}", new object[]
				{
					uri
				});
				HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
				if (httpWebResponse.StatusCode != HttpStatusCode.OK)
				{
					throw new Exception(string.Format("HTTP {0} from web source", httpWebResponse.StatusCode.ToString()));
				}
				Stream responseStream = httpWebResponse.GetResponseStream();
				reader = new XmlTextReader(responseStream);
			}
			MashupParseContext mashupParseContext = new MashupParseContext(reader);
			CrunchedFile crunchedFile = null;
			using (mashupParseContext)
			{
				while (mashupParseContext.reader.Read() && crunchedFile == null)
				{
					if (mashupParseContext.reader.NodeType == XmlNodeType.Element && mashupParseContext.reader.Name == CrunchedFile.crunchedFileTag)
					{
						crunchedFile = new CrunchedFile(mashupParseContext);
						break;
					}
				}
				mashupParseContext.Dispose();
			}
			if (crunchedFile == null)
			{
				throw new InvalidMashupFile(mashupParseContext, string.Format("{0} doesn't appear to be a valid crunched file.", uri));
			}
			return crunchedFile;
		}
		public string GetRelativeFilename()
		{
			return CrunchedFile.CrunchedFilename;
		}
		public void WriteXML()
		{
			Stream w = this.renderOutput.CreateFile(this.GetRelativeFilename(), "text/xml");
			XmlTextWriter xmlTextWriter = new XmlTextWriter(w, Encoding.UTF8);
			using (xmlTextWriter)
			{
				xmlTextWriter.Formatting = Formatting.Indented;
				xmlTextWriter.WriteStartDocument(true);
				this.WriteXML(xmlTextWriter);
				xmlTextWriter.Close();
			}
		}
		public void WriteXML(XmlTextWriter writer)
		{
			writer.WriteStartElement(CrunchedFile.crunchedFileTag);
			writer.WriteAttributeString("xmlns:rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
			writer.WriteAttributeString("xmlns:dc", "http://purl.org/dc/elements/1.1/");
			writer.WriteAttributeString("xmlns:cc", "http://web.resource.org/cc/");
			writer.WriteAttributeString("Version", "1.6");
			writer.WriteAttributeString(CrunchedFile.renderDateTag, DateTime.Now.ToString());
			if (this.sourceMashupFilename != null)
			{
				writer.WriteAttributeString(CrunchedFile.SourceMashupFilenameAttr, this.sourceMashupFilename);
			}
			if (this.permitComposition)
			{
				this.WritePermitCompositionLicense(writer);
			}
			writer.WriteStartElement("BoundsList");
			foreach (TileRectangle current in this.boundsList)
			{
				writer.WriteStartElement("Bounds");
				writer.WriteAttributeString("zoom", current.zoom.ToString());
				writer.WriteAttributeString("X0", current.TopLeft.TileX.ToString());
				writer.WriteAttributeString("Y0", current.TopLeft.TileY.ToString());
				writer.WriteAttributeString("X1", (current.BottomRight.TileX + 1).ToString());
				writer.WriteAttributeString("Y1", (current.BottomRight.TileY + 1).ToString());
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
			writer.WriteStartElement("CrunchedFileIdentifier");
			FodderSupport.WriteAppFodderString(writer, FodderSupport.MapCruncherAppIDString, "");
			writer.WriteEndElement();
			writer.WriteStartElement("MapCruncherAppVersion");
			writer.WriteAttributeString("version", MSR.CVE.BackMaker.Resources.Version.ApplicationVersionNumber);
			FodderSupport.WriteAppFodderString(writer, FodderSupport.MapCruncherAppIDString, "Version" + FodderSupport.DigitsToLetters(FodderSupport.ExtractDigits(MSR.CVE.BackMaker.Resources.Version.ApplicationVersionNumber)));
			writer.WriteEndElement();
			writer.WriteStartElement(LayerList.GetXMLTag());
			foreach (CrunchedLayer current2 in this.crunchedLayers)
			{
				current2.WriteXML(writer);
			}
			writer.WriteEndElement();
			writer.WriteEndElement();
		}
		private void WritePermitCompositionLicense(XmlTextWriter writer)
		{
			writer.WriteStartElement("rdf:RDF");
			writer.WriteComment("This element indicates that the present XML document is in the\r\npublic domain. This permits others to compose the contents of this" + CrunchedFile.CrunchedFilename + " document with other such documents to produce\r\ncomposite map presentations. This element does not address the\r\nusage of the image tiles referred to by this document.");
			writer.WriteStartElement("ThisDocument");
			writer.WriteAttributeString("xpath", "ancestor::CrunchUp");
			writer.WriteAttributeString("rdf:about", "");
			writer.WriteStartElement("cc:license");
			writer.WriteAttributeString("rdf:resource", "http://web.resource.org/cc/PublicDomain");
			writer.WriteEndElement();
			writer.WriteEndElement();
			writer.WriteStartElement("cc:License");
			writer.WriteAttributeString("rdf:about", "http://web.resource.org/cc/PublicDomain");
			writer.WriteStartElement("dc:title");
			writer.WriteString("public domain");
			writer.WriteEndElement();
			writer.WriteStartElement("dc:description");
			writer.WriteString("no copyright; everything is permitted without restriction");
			writer.WriteEndElement();
			writer.WriteEndElement();
			writer.WriteEndElement();
		}
		public void WriteSourceMapLegendFrames()
		{
			foreach (CrunchedLayer current in this.crunchedLayers)
			{
				current.WriteSourceMapLegendFrames(this.renderOutput);
			}
		}
	}
}
