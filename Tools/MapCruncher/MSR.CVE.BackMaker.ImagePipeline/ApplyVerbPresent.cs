using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class ApplyVerbPresent : Verb
	{
		public Present Evaluate(Present[] paramList)
		{
			if (paramList[0] is PresentFailureCode)
			{
				return new PresentFailureCode((PresentFailureCode)paramList[0], "ApplyVerbPresent");
			}
			if (!(paramList[0] is VerbPresent))
			{
				return PresentFailureCode.FailedCast(paramList[0], "ApplyVerbPresent");
			}
			Present[] array = new Present[paramList.Length - 1];
			Array.Copy(paramList, 1, array, 0, paramList.Length - 1);
			return ((VerbPresent)paramList[0]).Evaluate(array);
		}
		public void AccumulateRobustHash(IRobustHash hash)
		{
			hash.Accumulate("ApplyVerbPresent");
		}
	}
}
