using System;
using System.IO;
using System.Net;
namespace MSR.CVE.BackMaker
{
	public class S3Response
	{
		private WebResponse response;
		private string responseString;
		private S3Response(WebResponse response)
		{
			this.response = response;
			Stream responseStream = response.GetResponseStream();
			StreamReader streamReader = new StreamReader(responseStream);
			this.responseString = streamReader.ReadToEnd();
			streamReader.Close();
			responseStream.Close();
			response.Close();
		}
		public static S3Response Execute(WebRequest request)
		{
			WebResponse webResponse = request.GetResponse();
			return new S3Response(webResponse);
		}
	}
}
