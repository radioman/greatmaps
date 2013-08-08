using System;
using System.Collections.Generic;
using System.Threading;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class OpenDocumentSensitivePrioritizer : OpenDocumentStateObserverIfc
	{
		private class ODSPFutureSet : Dictionary<int, OpenDocumentSensitivePrioritizedFuture>
		{
		}
		private class DocToFuturesDict : Dictionary<IFuture, OpenDocumentSensitivePrioritizer.ODSPFutureSet>
		{
		}
		private SizeSensitiveCache openDocumentCache;
		private OpenDocumentSensitivePrioritizer.DocToFuturesDict docToFuturesDict = new OpenDocumentSensitivePrioritizer.DocToFuturesDict();
		public OpenDocumentSensitivePrioritizer(SizeSensitiveCache openDocumentCache)
		{
			this.openDocumentCache = openDocumentCache;
			openDocumentCache.AddCallback(this);
		}
		internal void Realizing(OpenDocumentSensitivePrioritizedFuture openDocumentSensitivePrioritizedFuture)
		{
			Monitor.Enter(this);
			try
			{
				IFuture openDocumentFuture = openDocumentSensitivePrioritizedFuture.GetOpenDocumentFuture();
				if (openDocumentFuture != null)
				{
					if (!this.docToFuturesDict.ContainsKey(openDocumentFuture))
					{
						this.docToFuturesDict[openDocumentFuture] = new OpenDocumentSensitivePrioritizer.ODSPFutureSet();
					}
					D.Assert(!this.docToFuturesDict[openDocumentFuture].ContainsKey(openDocumentSensitivePrioritizedFuture.identity));
					this.docToFuturesDict[openDocumentFuture][openDocumentSensitivePrioritizedFuture.identity] = openDocumentSensitivePrioritizedFuture;
					openDocumentSensitivePrioritizedFuture.DocumentStateChanged(this.openDocumentCache.Contains(openDocumentFuture));
				}
			}
			finally
			{
				Monitor.Exit(this);
			}
		}
		internal void Complete(OpenDocumentSensitivePrioritizedFuture openDocumentSensitivePrioritizedFuture)
		{
			Monitor.Enter(this);
			try
			{
				IFuture openDocumentFuture = openDocumentSensitivePrioritizedFuture.GetOpenDocumentFuture();
				if (openDocumentFuture != null)
				{
					bool condition = this.docToFuturesDict[openDocumentFuture].Remove(openDocumentSensitivePrioritizedFuture.identity);
					D.Assert(condition);
				}
			}
			finally
			{
				Monitor.Exit(this);
			}
		}
		public void DocumentStateChanged(IFuture documentFuture, bool isOpen)
		{
			Monitor.Enter(this);
			try
			{
				if (this.docToFuturesDict.ContainsKey(documentFuture))
				{
					foreach (OpenDocumentSensitivePrioritizedFuture current in this.docToFuturesDict[documentFuture].Values)
					{
						current.DocumentStateChanged(isOpen);
					}
				}
			}
			finally
			{
				Monitor.Exit(this);
			}
		}
	}
}
