using System;
namespace MSR.CVE.BackMaker
{
	public interface DocumentMutabilityControlIfc
	{
		void SetDocumentMutable(bool mutable);
		bool GetDocumentMutable();
	}
}
