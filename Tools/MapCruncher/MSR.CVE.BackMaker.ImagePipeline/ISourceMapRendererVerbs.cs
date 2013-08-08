using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public interface ISourceMapRendererVerbs
	{
		Verb GetOpenVerb();
		Verb GetRenderVerb(bool useDocumentTransparency);
		Verb GetFetchBoundsVerb();
		Verb GetImageDetailVerb();
		string GetRendererCredit();
	}
}
