using System;
using System.IO;
namespace MSR.CVE.BackMaker
{
	public interface RenderOutputMethod
	{
		Stream CreateFile(string relativePath, string contentType);
		Stream ReadFile(string relativePath);
		Uri GetUri(string relativePath);
		bool KnowFileExists(string outputFilename);
		FileIdentification GetFileIdentification(string relativePath);
		RenderOutputMethod MakeChildMethod(string subdir);
		void EmptyDirectory();
	}
}
