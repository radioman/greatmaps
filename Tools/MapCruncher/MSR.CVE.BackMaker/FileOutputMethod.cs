using System;
using System.IO;
namespace MSR.CVE.BackMaker
{
	public class FileOutputMethod : RenderOutputMethod
	{
		private string basePath;
		public FileOutputMethod(string basePath)
		{
			this.basePath = basePath;
		}
		public Stream CreateFile(string relativePath, string contentType)
		{
			string path = this.GetPath(relativePath);
			Directory.CreateDirectory(Path.GetDirectoryName(path));
			File.Delete(path);
			return new FileStream(path, FileMode.Create);
		}
		public Stream ReadFile(string relativePath)
		{
			string path = this.GetPath(relativePath);
			return new FileStream(path, FileMode.Open, FileAccess.Read);
		}
		public Uri GetUri(string relativePath)
		{
			return new UriBuilder("file", "", 0, Path.Combine(this.basePath, relativePath)).Uri;
		}
		public string GetPath(string relativePath)
		{
			return Path.Combine(this.basePath, relativePath.Replace('/', '\\'));
		}
		internal void CreateDirectory()
		{
			Directory.CreateDirectory(this.basePath);
		}
		public bool KnowFileExists(string relativePath)
		{
			string path = this.GetPath(relativePath);
			return File.Exists(path);
		}
		public RenderOutputMethod MakeChildMethod(string subdir)
		{
			return new FileOutputMethod(Path.Combine(this.basePath, subdir));
		}
		public FileIdentification GetFileIdentification(string relativePath)
		{
			string path = this.GetPath(relativePath);
			return FileOutputMethod.GetFileIdentificationStatic(path);
		}
		public static FileIdentification GetFileIdentificationStatic(string fullPathname)
		{
			FileIdentification result;
			try
			{
				FileStream fileStream = File.Open(fullPathname, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				long length = fileStream.Length;
				fileStream.Close();
				result = new FileIdentification(length);
			}
			catch (FileNotFoundException)
			{
				result = new FileIdentification(-1L);
			}
			catch (DirectoryNotFoundException)
			{
				result = new FileIdentification(-1L);
			}
			return result;
		}
		public void EmptyDirectory()
		{
			try
			{
				Directory.Delete(this.GetPath(""), true);
			}
			catch (DirectoryNotFoundException)
			{
			}
			this.CreateDirectory();
		}
	}
}
