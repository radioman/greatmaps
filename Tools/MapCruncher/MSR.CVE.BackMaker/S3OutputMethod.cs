using System;
using System.IO;
using System.Net;
using System.Web.Services.Protocols;
namespace MSR.CVE.BackMaker
{
	public class S3OutputMethod : RenderOutputMethod
	{
		private class S3PutClosure : MemoryStream
		{
			private S3OutputMethod s3OutputMethod;
			private string s3key;
			private string contentType;
			public S3PutClosure(S3OutputMethod s3OutputMethod, string s3key, string contentType)
			{
				this.s3OutputMethod = s3OutputMethod;
				this.s3key = s3key;
				this.contentType = contentType;
			}
			public override void Close()
			{
				int num = 3;
				for (int i = 0; i < num; i++)
				{
					bool flag = i == num - 1;
					try
					{
						HeaderList headerList = new HeaderList();
						headerList.Add("content-type", this.contentType);
						headerList.Add("x-amz-acl", "public-read");
						this.s3OutputMethod.s3adaptor.put(this.s3OutputMethod.bucketName, this.s3key, new S3Content(this.ToArray()), headerList);
						break;
					}
					catch (WebException)
					{
						if (flag)
						{
							throw;
						}
					}
					catch (SoapException)
					{
						if (flag)
						{
							throw;
						}
					}
				}
				base.Close();
			}
		}
		private S3Adaptor s3adaptor;
		private string bucketName;
		private string basePath;
		private HeapBool bucketCreated;
		public S3OutputMethod(S3Adaptor s3adaptor, string bucketName, string basePath)
		{
			this.s3adaptor = s3adaptor;
			this.bucketName = bucketName;
			this.basePath = basePath;
			this.bucketCreated = new HeapBool(false);
		}
		public S3OutputMethod(S3OutputMethod template)
		{
			this.s3adaptor = template.s3adaptor;
			this.bucketName = template.bucketName;
			this.basePath = template.basePath;
			this.bucketCreated = template.bucketCreated;
		}
		public Stream CreateFile(string relativePath, string contentType)
		{
			if (!this.bucketCreated.value)
			{
				this.s3adaptor.CreateBucket(this.bucketName);
				this.bucketCreated.value = true;
			}
			string path = this.GetPath(relativePath);
			return new S3OutputMethod.S3PutClosure(this, path, contentType);
		}
		public Stream ReadFile(string relativePath)
		{
			string path = this.GetPath(relativePath);
			HeaderList headers = new HeaderList();
			return this.s3adaptor.getStream(this.bucketName, path, headers);
		}
		public Uri GetUri(string relativePath)
		{
			string pathValue = Path.Combine(Path.Combine(this.bucketName, this.basePath), relativePath).Replace('\\', '/');
			return new UriBuilder("http", "s3.amazonaws.com", 80, pathValue).Uri;
		}
		private string GetPath(string relativePath)
		{
			string text = Path.Combine(this.basePath, relativePath);
			return text.Replace("\\", "/");
		}
		public bool KnowFileExists(string relativePath)
		{
			return false;
		}
		public RenderOutputMethod MakeChildMethod(string subdir)
		{
			return new S3OutputMethod(this)
			{
				basePath = this.GetPath(subdir)
			};
		}
		public FileIdentification GetFileIdentification(string relativePath)
		{
			return new FileIdentification(-1L);
		}
		public void EmptyDirectory()
		{
		}
	}
}
