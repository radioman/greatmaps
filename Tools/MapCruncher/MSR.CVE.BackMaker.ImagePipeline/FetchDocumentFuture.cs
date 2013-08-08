using System;
using System.Collections.Generic;
using System.IO;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class FetchDocumentFuture : FutureBase, IFuturePrototype
	{
		private IFuture documentFuture;
		private static Dictionary<string, string> _knownExtensions;
		private static Dictionary<string, string> knownExtensions
		{
			get
			{
				if (FetchDocumentFuture._knownExtensions == null)
				{
					FetchDocumentFuture._knownExtensions = new Dictionary<string, string>();
					FetchDocumentFuture._knownExtensions.Add("pdf", "FoxIt");
					FetchDocumentFuture._knownExtensions.Add("jpg", "WPF");
					FetchDocumentFuture._knownExtensions.Add("gif", "WPF");
					FetchDocumentFuture._knownExtensions.Add("png", "WPF");
					FetchDocumentFuture._knownExtensions.Add("wmf", "GDI");
					FetchDocumentFuture._knownExtensions.Add("emf", "GDI");
					FetchDocumentFuture._knownExtensions.Add("tif", "WPF");
					FetchDocumentFuture._knownExtensions.Add("tiff", "WPF");
					FetchDocumentFuture._knownExtensions.Add("bmp", "WPF");
				}
				return FetchDocumentFuture._knownExtensions;
			}
		}
		public FetchDocumentFuture(IFuture documentFuture)
		{
			this.documentFuture = documentFuture;
		}
		public override Present Realize(string refCredit)
		{
			Present present = this.documentFuture.Realize(refCredit);
			if (!(present is SourceDocument))
			{
				return PresentFailureCode.FailedCast(present, "FetchDocumentFuture");
			}
			SourceDocument sourceDocument = (SourceDocument)present;
			string filesystemAbsolutePath = sourceDocument.localDocument.GetFilesystemAbsolutePath();
			Present[] paramList = new Present[]
			{
				new StringParameter(filesystemAbsolutePath),
				new IntParameter(sourceDocument.localDocument.GetPageNumber())
			};
			string text = Path.GetExtension(filesystemAbsolutePath).ToLower();
			if (text[0] == '.')
			{
				text = text.Substring(1);
			}
			Verb verb = null;
			string a = null;
			if (FetchDocumentFuture.knownExtensions.ContainsKey(text))
			{
				a = FetchDocumentFuture.knownExtensions[text];
			}
			if (a == "FoxIt")
			{
				verb = new FoxitOpenVerb();
			}
			else
			{
				if (a == "WPF")
				{
					verb = new WPFOpenVerb();
				}
				else
				{
					if (a == "GDI")
					{
						verb = new GDIOpenVerb();
					}
				}
			}
			if (verb == null)
			{
				return new PresentFailureCode(new UnknownImageTypeException("Unknown file type " + text));
			}
			return verb.Evaluate(paramList);
		}
		public override void AccumulateRobustHash(IRobustHash hash)
		{
			hash.Accumulate("FetchDocumentFuture(");
			this.documentFuture.AccumulateRobustHash(hash);
			hash.Accumulate(")");
		}
		public static string[] GetKnownFileTypes()
		{
			return new List<string>(FetchDocumentFuture.knownExtensions.Keys).ToArray();
		}
	}
}
