using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Xml;
namespace MSR.CVE.BackMaker
{
	internal class SourceMapLegendFrame
	{
		public delegate ThumbnailRecord ThumbnailDelegate();
		private const string filenameAttr = "Filename";
		private const string urlAttr = "URL";
		private const string widthAttr = "Width";
		private const string heightAttr = "Height";
		private bool useLoadedSize;
		private Size loadedSize;
		private string filename;
		private string displayName;
		private SourceMapInfo sourceMapInfo;
		private List<LegendRecord> legendRecords;
		private SourceMapLegendFrame.ThumbnailDelegate thumbnailDelegate;
		public Size size
		{
			get
			{
				if (this.useLoadedSize)
				{
					return this.loadedSize;
				}
				Size result = new Size(250, 50);
				foreach (LegendRecord current in this.legendRecords)
				{
					result.Width = Math.Max(result.Width, current.imageDimensions.Width);
					result.Height += current.imageDimensions.Height;
				}
				ThumbnailRecord thumbnailRecord = this.thumbnailDelegate();
				if (thumbnailRecord != null)
				{
					result.Width = Math.Max(result.Width, thumbnailRecord.size.Width);
					result.Height += thumbnailRecord.size.Height;
				}
				return result;
			}
		}
		public SourceMapLegendFrame(Layer layer, SourceMap sourceMap, List<LegendRecord> legendRecords, SourceMapLegendFrame.ThumbnailDelegate thumbnailDelegate)
		{
			this.filename = RenderState.ForceValidFilename(string.Format("SourceMap_{0}_{1}.html", layer.displayName, sourceMap.displayName));
			this.displayName = sourceMap.displayName;
			this.sourceMapInfo = sourceMap.sourceMapInfo;
			this.legendRecords = legendRecords;
			this.thumbnailDelegate = thumbnailDelegate;
		}
		internal static string GetXMLTag()
		{
			return "SourceMapLegendFrame";
		}
		public SourceMapLegendFrame(MashupParseContext context)
		{
			XMLTagReader xMLTagReader = context.NewTagReader(SourceMapLegendFrame.GetXMLTag());
			this.filename = context.GetRequiredAttribute("Filename");
			this.loadedSize.Width = context.GetRequiredAttributeInt("Width");
			this.loadedSize.Height = context.GetRequiredAttributeInt("Height");
			this.useLoadedSize = true;
			xMLTagReader.SkipAllSubTags();
		}
		public void WriteXML(XmlTextWriter writer)
		{
			writer.WriteStartElement(SourceMapLegendFrame.GetXMLTag());
			writer.WriteAttributeString("Filename", this.filename);
			writer.WriteAttributeString("URL", this.GetURL());
			writer.WriteAttributeString("Width", this.size.Width.ToString(CultureInfo.InvariantCulture));
			writer.WriteAttributeString("Height", this.size.Height.ToString(CultureInfo.InvariantCulture));
			writer.WriteEndElement();
		}
		private string GetURL()
		{
			return string.Format("{0}/{1}", "legends", this.filename);
		}
		public void WriteSourceMapLegendFrame(RenderOutputMethod renderOutput)
		{
			Stream stream = renderOutput.MakeChildMethod("legends").CreateFile(this.filename, "text/html");
			StreamWriter streamWriter = new StreamWriter(stream);
			streamWriter.WriteLine("<html>");
			streamWriter.WriteLine(string.Format("<head><title>{0}</title></head>", this.displayName));
			streamWriter.WriteLine("<body>");
			streamWriter.WriteLine(string.Format("<h3>{0}</h3>", this.displayName));
			ThumbnailRecord thumbnailRecord = this.thumbnailDelegate();
			if (thumbnailRecord != null)
			{
				streamWriter.WriteLine(thumbnailRecord.WriteImgTag("../"));
			}
			if (this.sourceMapInfo.mapFileURL != "")
			{
				streamWriter.WriteLine(string.Format("<br>Map URL: <a href=\"{0}\">{0}</a>", this.sourceMapInfo.mapFileURL));
			}
			if (this.sourceMapInfo.mapHomePage != "")
			{
				streamWriter.WriteLine(string.Format("<br>Map Home Page: <a href=\"{0}\">{0}</a>", this.sourceMapInfo.mapHomePage));
			}
			if (this.sourceMapInfo.mapDescription != "")
			{
				streamWriter.WriteLine(string.Format("<p>{0}</p>", this.sourceMapInfo.mapDescription));
			}
			foreach (LegendRecord current in this.legendRecords)
			{
				streamWriter.WriteLine(string.Format("<br><img src=\"{0}\" width=\"{1}\" height=\"{2}\">", current.urlSuffix, current.imageDimensions.Width, current.imageDimensions.Height));
			}
			streamWriter.WriteLine("</body>");
			streamWriter.WriteLine("</html>");
			streamWriter.Close();
		}
	}
}
