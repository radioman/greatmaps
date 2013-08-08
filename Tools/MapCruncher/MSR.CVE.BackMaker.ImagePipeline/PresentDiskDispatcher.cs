using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class PresentDiskDispatcher
	{
		private enum AssociatedFileType
		{
			Image
		}
		private Dictionary<Type, bool> complainedSet = new Dictionary<Type, bool>();
		private static string outputExtension(ImageFormat imageFormat)
		{
			string result;
			try
			{
				result = ImageTypeMapper.ByImageFormat(imageFormat).extension;
			}
			catch (UnknownImageTypeException)
			{
				result = "png";
			}
			return result;
		}
		public void WriteObject(Present present, string path, out long length)
		{
			if (!(present is ImageRef))
			{
				if (!this.complainedSet.ContainsKey(present.GetType()))
				{
					this.complainedSet[present.GetType()] = true;
					D.Sayf(0, "No support for disk streams for type {0}", new object[]
					{
						present.GetType()
					});
				}
				length = 0L;
				return;
			}
			GDIBigLockedImage image = ((ImageRef)present).image;
			string text = path + "." + PresentDiskDispatcher.outputExtension(image.RawFormat);
			if (File.Exists(text))
			{
				throw new WriteObjectFailedException(text, "file exists", null);
			}
			try
			{
				image.Save(text);
			}
			catch (Exception innerEx)
			{
				throw new WriteObjectFailedException(text, "exception", innerEx);
			}
			Stream stream = new FileStream(path, FileMode.Create, FileAccess.Write);
			stream.Write(new byte[]
			{
				0
			}, 0, 1);
			StreamWriter streamWriter = new StreamWriter(stream);
			streamWriter.Write(text);
			streamWriter.Flush();
			length = PresentDiskDispatcher.FileLength(text) + stream.Length;
			stream.Close();
		}
		public Present ReadObject(string path, out long length)
		{
			PresentDiskDispatcher.AssociatedFileType associatedFileType;
			string text;
			this.ReadAssociatedFileName(path, out associatedFileType, out text, out length);
			D.Assert(associatedFileType == PresentDiskDispatcher.AssociatedFileType.Image);
			length += PresentDiskDispatcher.FileLength(text);
			GDIBigLockedImage image = GDIBigLockedImage.FromFile(text);
			return new ImageRef(new ImageRefCounted(image));
		}
		private void ReadAssociatedFileName(string cacheControlFilePath, out PresentDiskDispatcher.AssociatedFileType associatedFileType, out string associatedFileName, out long cacheControlFileLength)
		{
			Stream stream = new FileStream(cacheControlFilePath, FileMode.Open, FileAccess.Read);
			using (stream)
			{
				cacheControlFileLength = stream.Length;
				byte[] array = new byte[1];
				using (stream)
				{
					int num = stream.Read(array, 0, 1);
					if (num != 1)
					{
						throw new IOException("No type field at beginning of file.");
					}
					associatedFileType = (PresentDiskDispatcher.AssociatedFileType)array[0];
					PresentDiskDispatcher.AssociatedFileType associatedFileType2 = associatedFileType;
					if (associatedFileType2 != PresentDiskDispatcher.AssociatedFileType.Image)
					{
						throw new IOException("Unrecognized type field.");
					}
					string text = new StreamReader(stream).ReadToEnd();
					associatedFileName = text;
				}
			}
		}
		private static long FileLength(string path)
		{
			FileStream fileStream = File.Open(path, FileMode.Open);
			long length = fileStream.Length;
			fileStream.Close();
			return length;
		}
		public long CacheFileLength(string cacheControlFilePath)
		{
			PresentDiskDispatcher.AssociatedFileType associatedFileType;
			string text;
			long num;
			this.ReadAssociatedFileName(cacheControlFilePath, out associatedFileType, out text, out num);
			long num2 = 0L;
			if (text != null)
			{
				num2 = PresentDiskDispatcher.FileLength(text);
			}
			return num + num2;
		}
		public void DeleteCacheFile(string cacheControlFilePath)
		{
			PresentDiskDispatcher.AssociatedFileType associatedFileType;
			string text;
			long num;
			this.ReadAssociatedFileName(cacheControlFilePath, out associatedFileType, out text, out num);
			if (text != null)
			{
				File.Delete(text);
			}
			File.Delete(cacheControlFilePath);
		}
	}
}
