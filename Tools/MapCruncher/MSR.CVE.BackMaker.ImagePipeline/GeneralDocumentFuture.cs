using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class GeneralDocumentFuture
	{
		private IDocumentFuture _documentFuture;
		public IDocumentFuture documentFuture
		{
			get
			{
				return this._documentFuture;
			}
		}
		public void WriteXML(MashupWriteContext context, string pathBase)
		{
			context.writer.WriteStartElement(GeneralDocumentFuture.GetXMLTag());
			this._documentFuture.WriteXML(context, pathBase);
			context.writer.WriteEndElement();
		}
		public GeneralDocumentFuture(IDocumentFuture documentFuture)
		{
			this._documentFuture = documentFuture;
		}
		public GeneralDocumentFuture(MashupParseContext context, string pathBase)
		{
			XMLTagReader xMLTagReader = context.NewTagReader(GeneralDocumentFuture.GetXMLTag());
			while (xMLTagReader.FindNextStartTag())
			{
				if (xMLTagReader.TagIs(FutureDocumentFromFilesystem.GetXMLTag()))
				{
					if (this._documentFuture != null)
					{
						throw new InvalidMashupFile(context, "Too many specs in " + GeneralDocumentFuture.GetXMLTag());
					}
					this._documentFuture = new FutureDocumentFromFilesystem(context, pathBase);
				}
				else
				{
					if (xMLTagReader.TagIs(FutureDocumentFromUri.GetXMLTag()))
					{
						if (this._documentFuture != null)
						{
							throw new InvalidMashupFile(context, "Too many specs in " + GeneralDocumentFuture.GetXMLTag());
						}
						this._documentFuture = new FutureDocumentFromUri(context);
					}
				}
			}
			if (this._documentFuture == null)
			{
				throw new InvalidMashupFile(context, "No spec in " + GeneralDocumentFuture.GetXMLTag());
			}
		}
		internal static string GetXMLTag()
		{
			return "Document";
		}
		public IFuture GetSynchronousFuture(CachePackage cachePackage)
		{
			return new MemCacheFuture(cachePackage.documentFetchCache, this.documentFuture);
		}
		public IFuture GetAsynchronousFuture(CachePackage cachePackage)
		{
			return new MemCacheFuture(cachePackage.asyncCache, Asynchronizer.MakeFuture(cachePackage.computeAsyncScheduler, this.GetSynchronousFuture(cachePackage)));
		}
		public SourceDocument RealizeSynchronously(CachePackage cachePackage)
		{
			Present present = this.GetSynchronousFuture(cachePackage).Realize("SourceDocument.RealizeSynchronously");
			if (present is SourceDocument)
			{
				SourceDocument sourceDocument = (SourceDocument)present;
				D.Assert(sourceDocument.localDocument != null, "We waited for document to arrive synchronously.");
				return sourceDocument;
			}
			throw ((PresentFailureCode)present).exception;
		}
		internal void AccumulateRobustHash(IRobustHash hash)
		{
			this.documentFuture.AccumulateRobustHash(hash);
		}
	}
}
