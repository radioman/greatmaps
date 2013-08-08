using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class GDIOpenVerb : Verb
	{
		public Present Evaluate(Present[] paramList)
		{
			StringParameter stringParameter = (StringParameter)paramList[0];
			IntParameter intParameter = (IntParameter)paramList[1];
			D.Assert(paramList.Length == 2);
			D.Assert(intParameter.value == 0);
			Present result;
			try
			{
				result = new GDIOpenDocument(stringParameter.value);
			}
			catch (Exception ex)
			{
				result = new PresentFailureCode(ex);
			}
			return result;
		}
		public void AccumulateRobustHash(IRobustHash hash)
		{
			hash.Accumulate("GDIOpenVerb");
		}
		public string GetRendererName()
		{
			return "GDI";
		}
		public string GetRendererCredit()
		{
			return null;
		}
	}
}
