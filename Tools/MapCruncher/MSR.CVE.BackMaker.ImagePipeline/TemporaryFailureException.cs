using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class TemporaryFailureException : Exception
	{
		public TemporaryFailureException(string msg) : base(msg)
		{
		}
	}
}
