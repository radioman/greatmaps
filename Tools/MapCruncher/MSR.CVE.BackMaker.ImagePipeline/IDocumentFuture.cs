using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public interface IDocumentFuture : IFuture, IRobustlyHashable, IFuturePrototype
	{
		string GetDefaultDisplayName();
		void WriteXML(MashupWriteContext context, string pathBase);
	}
}
