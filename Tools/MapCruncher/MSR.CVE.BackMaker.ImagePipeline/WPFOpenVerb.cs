using System;
using System.IO;
using System.Windows.Forms;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	internal class WPFOpenVerb : Verb
	{
		public Present Evaluate(Present[] paramList)
		{
			StringParameter stringParameter = (StringParameter)paramList[0];
			IntParameter intParameter = (IntParameter)paramList[1];
			D.Assert(paramList.Length == 2);
			Present result;
			try
			{
				result = new WPFOpenDocument(stringParameter.value, intParameter.value);
			}
			catch (DllNotFoundException ex)
			{
				MessageBox.Show("It appears that .Net 3.0 is not installed on this machine. Please install .Net 3.0 and restart the application.", "Missing Dependency");
				result = new PresentFailureCode(ex);
			}
			catch (FileNotFoundException ex2)
			{
				if (ex2.Source == "PresentationCore")
				{
					MessageBox.Show("It appears that .Net 3.0 is not installed on this machine. Please install .Net 3.0 and restart the application.", "Missing Dependency");
				}
				result = new PresentFailureCode(ex2);
			}
			catch (Exception ex3)
			{
				result = new PresentFailureCode(ex3);
			}
			return result;
		}
		public void AccumulateRobustHash(IRobustHash hash)
		{
			hash.Accumulate("WPFOpenVerb");
		}
		public string GetRendererName()
		{
			return "WPF";
		}
		public string GetRendererCredit()
		{
			return null;
		}
	}
}
