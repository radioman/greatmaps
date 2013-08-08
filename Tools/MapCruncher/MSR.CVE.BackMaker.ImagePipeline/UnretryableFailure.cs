using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class UnretryableFailure : PresentFailureCode
	{
		public UnretryableFailure(Exception ex) : base(ex)
		{
		}
	}
}
