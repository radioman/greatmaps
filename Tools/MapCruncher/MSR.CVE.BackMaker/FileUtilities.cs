using System;
using System.IO;
namespace MSR.CVE.BackMaker
{
	public static class FileUtilities
	{
		public static string MakeTempFilename(string directory, string fileNamePrefix)
		{
			int num = new Random().Next();
			string text;
			while (true)
			{
				text = Path.Combine(directory, string.Format("{0}-{1}", fileNamePrefix, num.ToString()));
				if (!File.Exists(text))
				{
					break;
				}
				num++;
			}
			return text;
		}
	}
}
