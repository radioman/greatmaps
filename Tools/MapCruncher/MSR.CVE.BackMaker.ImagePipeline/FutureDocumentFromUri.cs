using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Security.Cryptography;
using System.Text;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class FutureDocumentFromUri : FutureBase, IDocumentFuture, IFuture, IRobustlyHashable, IFuturePrototype
	{
		private Uri documentUri;
		private int pageNumber;
		private static string FetchedDocumentUriAttr = "Uri";
		private static string FetchedDocumentPageNumberAttr = "PageNumber";
		public FutureDocumentFromUri(Uri documentUri, int pageNumber)
		{
			this.documentUri = documentUri;
			this.pageNumber = pageNumber;
		}
		public override Present Realize(string refCredit)
		{
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(this.documentUri);
			httpWebRequest.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.Revalidate);
			D.Sayf(0, "Fetching {0}", new object[]
			{
				this.documentUri
			});
			HttpWebResponse httpWebResponse;
			try
			{
				httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
			}
			catch (WebException ex)
			{
				throw new Exception(string.Format("timeout waiting for url ({0})", ex.ToString()));
			}
			if (httpWebResponse.StatusCode != HttpStatusCode.OK)
			{
				throw new Exception(string.Format("HTTP {0} from web source", httpWebResponse.StatusCode.ToString()));
			}
			Stream responseStream = httpWebResponse.GetResponseStream();
			HashAlgorithm hashAlgorithm = new SHA1CryptoServiceProvider();
			string text = FileUtilities.MakeTempFilename(FutureDocumentFromUri.MakeDownloadCacheDir(), "Download");
			Stream outputStream = new FileStream(text, FileMode.CreateNew);
			StreamTee streamTee = new StreamTee(responseStream, outputStream);
			byte[] buffer = hashAlgorithm.ComputeHash(streamTee);
			streamTee.Close();
			string arg = this.BytesToHexString(buffer);
			string text2 = Path.Combine(FutureDocumentFromUri.MakeDownloadCacheDir(), string.Format("Hash-{0}.{1}", arg, ImageTypeMapper.ByMimeType(httpWebResponse.ContentType).extension));
			if (File.Exists(text2))
			{
				File.Delete(text);
			}
			else
			{
				File.Move(text, text2);
			}
			httpWebResponse.Close();
			return new SourceDocument(new LocalDocumentDescriptor(text2, this.pageNumber));
		}
		private static string MakeDownloadCacheDir()
		{
			string text = Path.Combine(Environment.GetEnvironmentVariable("TMP"), "mapcache\\downloads\\");
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			return text;
		}
		private string BytesToHexString(byte[] buffer)
		{
			StringBuilder stringBuilder = new StringBuilder(buffer.Length * 2);
			for (int i = 0; i < buffer.Length; i++)
			{
				byte b = buffer[i];
				stringBuilder.Append(b.ToString("X2"));
			}
			return stringBuilder.ToString();
		}
		public static string GetXMLTag()
		{
			return "UriDocument";
		}
		public void WriteXML(MashupWriteContext wc, string pathBase)
		{
			wc.writer.WriteStartElement(FutureDocumentFromUri.GetXMLTag());
			wc.writer.WriteAttributeString(FutureDocumentFromUri.FetchedDocumentUriAttr, this.documentUri.ToString());
			wc.writer.WriteAttributeString(FutureDocumentFromUri.FetchedDocumentPageNumberAttr, this.pageNumber.ToString(CultureInfo.InvariantCulture));
			wc.writer.WriteEndElement();
		}
		public override void AccumulateRobustHash(IRobustHash hash)
		{
			hash.Accumulate(this.documentUri.ToString());
			hash.Accumulate(this.pageNumber);
		}
		public FutureDocumentFromUri(MashupParseContext context)
		{
			XMLTagReader xMLTagReader = context.NewTagReader(FutureDocumentFromUri.GetXMLTag());
			this.documentUri = new Uri(context.GetRequiredAttribute(FutureDocumentFromUri.FetchedDocumentUriAttr));
			this.pageNumber = context.GetRequiredAttributeInt(FutureDocumentFromUri.FetchedDocumentPageNumberAttr);
			xMLTagReader.SkipAllSubTags();
		}
		public string GetDefaultDisplayName()
		{
			return this.documentUri.ToString();
		}
	}
}
