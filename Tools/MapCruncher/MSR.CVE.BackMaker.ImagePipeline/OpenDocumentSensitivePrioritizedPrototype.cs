using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class OpenDocumentSensitivePrioritizedPrototype : IFuturePrototype
	{
		private OpenDocumentSensitivePrioritizer prioritizer;
		private IFuturePrototype prototype;
		private IFuture openDocumentFuture;
		public OpenDocumentSensitivePrioritizedPrototype(OpenDocumentSensitivePrioritizer prioritizer, IFuturePrototype prototype, IFuture openDocumentFuture)
		{
			this.prioritizer = prioritizer;
			this.prototype = prototype;
			this.openDocumentFuture = openDocumentFuture;
		}
		public IFuture Curry(ParamDict paramDict)
		{
			return new OpenDocumentSensitivePrioritizedFuture(this.prioritizer, this.prototype.Curry(paramDict), this.openDocumentFuture);
		}
	}
}
