using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public interface IDocumentSource
	{
		IFuture GetOpenDocumentFuture(FutureFeatures features);
	}
}
