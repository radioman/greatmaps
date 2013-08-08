using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class FutureDocumentFromFilesystem : FutureBase, IDocumentFuture, IFuture, IRobustlyHashable, IFuturePrototype
	{
		private string path;
		private int pageNumber;
		private DateTime lastWriteTime;
		private static string FilenameAttr = "Filename";
		private static string PageNumberAttr = "PageNumber";
		public FutureDocumentFromFilesystem(string path, int pageNumber)
		{
			this.path = path;
			this.pageNumber = pageNumber;
			File.GetLastWriteTime(path).ToUniversalTime();
			this.ValidateFilename();
		}
		public FutureDocumentFromFilesystem(MashupParseContext context, string pathBase)
		{
			XMLTagReader xMLTagReader = context.NewTagReader(FutureDocumentFromFilesystem.GetXMLTag());
			string requiredAttribute = context.GetRequiredAttribute(FutureDocumentFromFilesystem.FilenameAttr);
			this.path = Path.Combine(pathBase, requiredAttribute);
			this.pageNumber = context.GetRequiredAttributeInt(FutureDocumentFromFilesystem.PageNumberAttr);
			xMLTagReader.SkipAllSubTags();
			this.ValidateFilename();
		}
		public void WriteXML(MashupWriteContext context, string pathBase)
		{
			string value = FutureDocumentFromFilesystem.MakeRelativePath(pathBase, this.path);
			context.writer.WriteStartElement(FutureDocumentFromFilesystem.GetXMLTag());
			context.writer.WriteAttributeString(FutureDocumentFromFilesystem.FilenameAttr, value);
			context.writer.WriteAttributeString(FutureDocumentFromFilesystem.PageNumberAttr, this.pageNumber.ToString(CultureInfo.InvariantCulture));
			context.writer.WriteEndElement();
		}
		public static string MakeRelativePath(string pathBase, string path)
		{
			D.Assert(Path.IsPathRooted(path));
			string text;
			if (pathBase == null || pathBase == "")
			{
				text = "";
			}
			else
			{
				text = Path.GetFullPath(pathBase);
			}
			string[] array = text.Split(new char[]
			{
				Path.DirectorySeparatorChar
			});
			string fullPath = Path.GetFullPath(path);
			string[] array2 = fullPath.Split(new char[]
			{
				Path.DirectorySeparatorChar
			});
			if (array[0] != array2[0])
			{
				return fullPath;
			}
			int num = 0;
			while (num < Math.Min(array.Length, text.Length) && !(array[num] != array2[num]))
			{
				num++;
			}
			int num2 = array.Length - num;
			List<string> list = new List<string>();
			for (int i = 0; i < num2; i++)
			{
				list.Add("..");
			}
			for (int j = num; j < array2.Length; j++)
			{
				list.Add(array2[j]);
			}
			return string.Join("" + Path.DirectorySeparatorChar, list.ToArray());
		}
		private void ValidateFilename()
		{
			if (!File.Exists(this.path))
			{
				throw new InvalidFileContentsException(string.Format("Document reference to {0} invalid", this.path));
			}
		}
		public override Present Realize(string refCredit)
		{
			return new SourceDocument(new LocalDocumentDescriptor(this.path, this.pageNumber));
		}
		public override void AccumulateRobustHash(IRobustHash hash)
		{
			hash.Accumulate("FutureDocumentFromFilesystem(");
			hash.Accumulate(this.path);
			hash.Accumulate(this.pageNumber);
			hash.Accumulate(this.lastWriteTime.ToBinary());
			hash.Accumulate(")");
		}
		public string GetDefaultDisplayName()
		{
			return Path.GetFileNameWithoutExtension(this.path);
		}
		internal static string GetXMLTag()
		{
			return "FileDocument";
		}
	}
}
