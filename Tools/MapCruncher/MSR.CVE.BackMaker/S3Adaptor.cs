using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
namespace MSR.CVE.BackMaker
{
	public class S3Adaptor
	{
		private string accessKeyId;
		private string secretAccessKey;
		public S3Adaptor(string accessKeyId, string secretAccessKey)
		{
			this.accessKeyId = accessKeyId;
			this.secretAccessKey = secretAccessKey;
		}
		public S3Response put(string bucket, string key, S3Content content, HeaderList headers)
		{
			WebRequest webRequest = this.BuildWebRequest("PUT", bucket + "/" + this.EncodeKeyForSignature(key), headers);
			webRequest.ContentLength = (long)content.Bytes.Length;
			Stream requestStream = webRequest.GetRequestStream();
			requestStream.Write(content.Bytes, 0, content.Bytes.Length);
			requestStream.Close();
			return S3Response.Execute(webRequest);
		}
		public Stream getStream(string bucket, string key, HeaderList headers)
		{
			WebRequest webRequest = this.BuildWebRequest("GET", bucket + "/" + this.EncodeKeyForSignature(key), headers);
			WebResponse response = webRequest.GetResponse();
			return response.GetResponseStream();
		}
		public void CreateBucket(string bucketName)
		{
			WebRequest webRequest = this.BuildWebRequest("PUT", bucketName, new HeaderList());
			webRequest.ContentLength = 0L;
			webRequest.GetRequestStream().Close();
			S3Response.Execute(webRequest);
		}
		private string EncodeKeyForSignature(string key)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < key.Length; i++)
			{
				char c = key[i];
				if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '/' || c == '.')
				{
					stringBuilder.Append(c);
				}
				else
				{
					string text = string.Format("%{0:x2}", (int)c);
					D.Assert(text.Length == 3);
					stringBuilder.Append(text);
				}
			}
			return stringBuilder.ToString();
		}
		private WebRequest BuildWebRequest(string method, string objectPath, HeaderList protoHeaders)
		{
			UriBuilder uriBuilder = new UriBuilder("http", "s3.amazonaws.com", 80, objectPath);
			WebRequest webRequest = WebRequest.Create(uriBuilder.ToString());
			((HttpWebRequest)webRequest).AllowWriteStreamBuffering = false;
			webRequest.Method = method;
			HeaderList headerList = new HeaderList(protoHeaders);
			headerList.AddHeaderIfAbsent("x-amz-date", DateTime.UtcNow.ToString("ddd, dd MMM yyyy HH:mm:ss ", CultureInfo.InvariantCulture) + "GMT");
			for (int i = 0; i < headerList.Count; i++)
			{
				string a = headerList.Keys[i];
				string contentType = headerList.Values[i];
				if (a == "content-type")
				{
					webRequest.ContentType = contentType;
				}
				else
				{
					webRequest.Headers.Add(headerList.Keys[i], headerList.Values[i]);
				}
			}
			StringBuilder stringBuilder = new StringBuilder();
			HeaderList headerList2 = new HeaderList();
			foreach (string current in headerList.Keys)
			{
				string text = current.ToLower();
				if (text == "content-type" || text == "content-md5" || text == "date" || text.StartsWith("x-amz-"))
				{
					headerList2.Add(text, headerList[current]);
				}
			}
			if (headerList2.ContainsKey("x-amz-date"))
			{
				headerList2["date"] = "";
			}
			if (!headerList2.ContainsKey("content-type"))
			{
				headerList2["content-type"] = "";
			}
			if (!headerList2.ContainsKey("content-md5"))
			{
				headerList2["content-md5"] = "";
			}
			stringBuilder.Append(webRequest.Method);
			stringBuilder.Append("\n");
			for (int j = 0; j < headerList2.Count; j++)
			{
				if (headerList2.Keys[j].StartsWith("x-amz-"))
				{
					stringBuilder.Append(headerList2.Keys[j]);
					stringBuilder.Append(":");
					stringBuilder.Append(headerList2.Values[j].Trim());
				}
				else
				{
					stringBuilder.Append(headerList2.Values[j]);
				}
				stringBuilder.Append("\n");
			}
			stringBuilder.Append("/");
			stringBuilder.Append(objectPath);
			string text2 = stringBuilder.ToString();
			Encoding encoding = new UTF8Encoding();
			HMACSHA1 hMACSHA = new HMACSHA1(encoding.GetBytes(this.secretAccessKey));
			byte[] inArray = hMACSHA.ComputeHash(encoding.GetBytes(text2.ToCharArray()));
			string arg = Convert.ToBase64String(inArray);
			webRequest.Headers.Add("Authorization", string.Format("AWS {0}:{1}", this.accessKeyId, arg));
			return webRequest;
		}
	}
}
