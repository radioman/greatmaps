using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class WriteObjectFailedException : Exception
	{
		public WriteObjectFailedException(string filename, string message, Exception innerEx) : base(string.Format("{0}: file {1}", message, filename), innerEx)
		{
		}
	}
}
