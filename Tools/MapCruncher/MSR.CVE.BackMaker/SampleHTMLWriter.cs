using System;
using System.IO;
namespace MSR.CVE.BackMaker
{
	public class SampleHTMLWriter
	{
		public delegate void PostMessageDelegate(string message);
		private const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
		public static string SampleHTMLFilename = "SamplePage.html";
		public static string SampleKeyFilename = "SampleKey.html";
		public static Uri Write(Mashup mashup, SampleHTMLWriter.PostMessageDelegate postMessage, RenderOutputMethod renderOutput)
		{
			string arg = "";
			try
			{
				SampleHTMLWriter.WriteMain(mashup, renderOutput, SampleHTMLWriter.SampleHTMLFilename);
				SampleHTMLWriter.WriteKey(mashup, renderOutput, SampleHTMLWriter.SampleKeyFilename);
				return renderOutput.GetUri(SampleHTMLWriter.SampleHTMLFilename);
			}
			catch (Exception ex)
			{
				postMessage(string.Format("Couldn't write {0}: {1}", arg, ex.ToString()));
			}
			return null;
		}
		private static void WriteMain(Mashup mashup, RenderOutputMethod renderOutput, string fileName)
		{
			TextWriter textWriter = new StreamWriter(renderOutput.CreateFile(fileName, "text/html"));
			string showDefaultLayerName = SampleHTMLWriter.ReferenceName(mashup.layerList.First.GetDisplayName());
			string text = "";
			string arg = " checked";
			foreach (Layer current in mashup.layerList)
			{
				string arg2 = string.Format("<input type=\"checkbox\" id=\"checkbox:{0}\" onClick=\"javascript:ToggleLayer('{0}', this.checked);\"{1}>{0}", SampleHTMLWriter.ReferenceName(current.GetDisplayName()), arg);
				arg = "";
				string arg3 = string.Format("<a href=\"javascript:crunchedLayerManager.layerList.find('{0}').SetDefaultView(map);\"><font size=\"-1\">center here</font></a>", SampleHTMLWriter.ReferenceName(current.GetDisplayName()));
				text += string.Format("{0}&nbsp;{1}&nbsp;|", arg2, arg3);
			}
			textWriter.Write(SampleHTMLWriterConstants.Body(showDefaultLayerName, text, CrunchedFile.CrunchedFilename));
			textWriter.Close();
		}
		public static void WriteKey(Mashup mashup, RenderOutputMethod renderOutput, string fileName)
		{
			TextWriter textWriter = new StreamWriter(renderOutput.CreateFile(fileName, "text/html"));
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(mashup.GetFilename());
			textWriter.WriteLine("<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\">");
			textWriter.WriteLine(string.Format("<html><head><title>{0}</title></head>", fileNameWithoutExtension));
			textWriter.WriteLine(string.Format("<body><h1>{0}</h1>", fileNameWithoutExtension));
			foreach (Layer current in mashup.layerList)
			{
				textWriter.WriteLine(string.Format("<p><h2><a name=\"{0}\">{1}</a></h2>", SampleHTMLWriter.ReferenceName(current.displayName), current.displayName));
				foreach (SourceMap current2 in current)
				{
					textWriter.WriteLine(string.Format("<p><h3>{0}</h3>", current2.displayName));
					if (current2.sourceMapInfo.mapHomePage != "")
					{
						textWriter.WriteLine(string.Format("<a href=\"{0}\">Home page</a>", current2.sourceMapInfo.mapHomePage));
					}
					if (current2.sourceMapInfo.mapFileURL != "")
					{
						textWriter.WriteLine(string.Format(" <a href=\"{0}\">Map URL</a>", current2.sourceMapInfo.mapFileURL));
					}
					textWriter.WriteLine(string.Format("<br>{0}", current2.sourceMapInfo.mapDescription));
				}
				textWriter.WriteLine("<hr>");
			}
			textWriter.WriteLine("</body></html>");
			textWriter.Close();
		}
		public static string ReferenceName(string displayName)
		{
			string text = "";
			for (int i = 0; i < displayName.Length; i++)
			{
				char c = displayName[i];
				if ("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".IndexOf(c) >= 0)
				{
					text += c;
				}
			}
			return text;
		}
	}
}
