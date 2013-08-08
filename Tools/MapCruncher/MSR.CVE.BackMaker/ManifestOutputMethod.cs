using System;
using System.IO;
namespace MSR.CVE.BackMaker
{
	public class ManifestOutputMethod : RenderOutputMethod
	{
		private class CreateCompleteClosure : StreamFilter
		{
			private ManifestOutputMethod manifestOutputMethod;
			private string path;
			private bool closed;
			public CreateCompleteClosure(Stream baseStream, ManifestOutputMethod manifestOutputMethod, string path) : base(baseStream)
			{
				this.manifestOutputMethod = manifestOutputMethod;
				this.path = path;
			}
			public override void Close()
			{
				if (!this.closed)
				{
					long length = base.Length;
					base.Close();
					this.closed = true;
					this.manifestOutputMethod.manifest.Add(this.path, length);
				}
			}
		}
		private string basePath;
		private RenderOutputMethod baseMethod;
		private Manifest manifest;
		public ManifestOutputMethod(RenderOutputMethod baseMethod)
		{
			this.basePath = "";
			this.baseMethod = baseMethod;
			this.manifest = new Manifest(baseMethod);
		}
		private ManifestOutputMethod(RenderOutputMethod baseMethod, string basePath, Manifest manifest)
		{
			this.baseMethod = baseMethod;
			this.basePath = basePath;
			this.manifest = manifest;
		}
		public Stream CreateFile(string relativePath, string contentType)
		{
			Stream baseStream = this.baseMethod.CreateFile(relativePath, contentType);
			return new ManifestOutputMethod.CreateCompleteClosure(baseStream, this, this.GetPath(relativePath));
		}
		public Stream ReadFile(string relativePath)
		{
			return this.baseMethod.ReadFile(relativePath);
		}
		public Uri GetUri(string relativePath)
		{
			return this.baseMethod.GetUri(relativePath);
		}
		public bool KnowFileExists(string outputFilename)
		{
			string path = this.GetPath(outputFilename);
			Manifest.ManifestRecord manifestRecord = this.manifest.FindFirstEqual(path);
			return manifestRecord.fileExists;
		}
		public FileIdentification GetFileIdentification(string relativePath)
		{
			string path = this.GetPath(relativePath);
			Manifest.ManifestRecord manifestRecord = this.manifest.FindFirstEqual(path);
			if (!manifestRecord.fileExists)
			{
				return new FileIdentification(-1L);
			}
			return new FileIdentification(manifestRecord.fileLength);
		}
		public RenderOutputMethod MakeChildMethod(string subdir)
		{
			return new ManifestOutputMethod(this.baseMethod.MakeChildMethod(subdir), this.GetPath(subdir), this.manifest);
		}
		public void EmptyDirectory()
		{
			Manifest.ManifestRecord manifestRecord = this.manifest.FindFirstGreaterEqual(this.basePath);
			while (!manifestRecord.IsTailRecord && manifestRecord.path.StartsWith(this.basePath))
			{
				if (manifestRecord.fileExists)
				{
					this.manifest.Remove(manifestRecord.path);
				}
				manifestRecord = this.manifest.FindFirstGreaterThan(manifestRecord.path);
			}
			this.CommitChanges();
			this.baseMethod.EmptyDirectory();
		}
		private string GetPath(string relativePath)
		{
			return this.basePath + "/" + relativePath;
		}
		public void CommitChanges()
		{
			this.manifest.CommitChanges();
		}
		public void Test_SetSplitThreshold(int splitThreshold)
		{
			this.manifest.Test_SetSplitThreshold(splitThreshold);
		}
	}
}
