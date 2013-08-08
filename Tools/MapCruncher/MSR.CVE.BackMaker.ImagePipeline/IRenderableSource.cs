using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public interface IRenderableSource : IComparable, IDisplayableSource
	{
		string GetSourceMapDisplayName();
		IFuture GetOpenDocumentFuture(FutureFeatures features);
	}
}
