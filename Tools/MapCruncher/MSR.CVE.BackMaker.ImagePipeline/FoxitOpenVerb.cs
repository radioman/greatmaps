using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class FoxitOpenVerb : Verb
	{
		public Present Evaluate(Present[] paramList)
		{
			StringParameter stringParameter = (StringParameter)paramList[0];
			IntParameter intParameter = (IntParameter)paramList[1];
			D.Assert(paramList.Length == 2);
			Present result;
			try
			{
				result = new FoxitOpenDocument(stringParameter.value, intParameter.value);
			}
			catch (Exception ex)
			{
				result = new PresentFailureCode(ex);
			}
			return result;
		}
		public void AccumulateRobustHash(IRobustHash hash)
		{
			hash.Accumulate("FoxitOpenVerb");
		}
		public string GetRendererName()
		{
			return "FoxIt";
		}
	}
}
