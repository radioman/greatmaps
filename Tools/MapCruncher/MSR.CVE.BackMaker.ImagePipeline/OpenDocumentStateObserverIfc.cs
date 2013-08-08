using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	internal interface OpenDocumentStateObserverIfc
	{
		void DocumentStateChanged(IFuture documentFuture, bool isOpen);
	}
}
