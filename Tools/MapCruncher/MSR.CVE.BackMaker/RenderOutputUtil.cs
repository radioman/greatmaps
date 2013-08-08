using MSR.CVE.BackMaker.ImagePipeline;
using System;
using System.Drawing.Imaging;
using System.IO;
namespace MSR.CVE.BackMaker
{
	public static class RenderOutputUtil
	{
		public static void CopyFile(string sourcePath, RenderOutputMethod renderOutput, string relativeDestPath, string mimeType)
		{
			FileIdentification fileIdentificationStatic = FileOutputMethod.GetFileIdentificationStatic(sourcePath);
			FileIdentification fileIdentification = renderOutput.GetFileIdentification(relativeDestPath);
			if (fileIdentificationStatic.CompareTo(fileIdentification) == 0)
			{
				return;
			}
			Stream stream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			Stream stream2 = renderOutput.CreateFile(relativeDestPath, mimeType);
			byte[] buffer = new byte[65536];
			while (true)
			{
				int num = stream.Read(buffer, 0, 65536);
				if (num == 0)
				{
					break;
				}
				stream2.Write(buffer, 0, num);
			}
			stream.Close();
			stream2.Close();
		}
		public static void SaveImage(ImageRef imageRef, RenderOutputMethod renderOutput, string relativeDestPath, ImageFormat imageFormat)
		{
			Stream stream = renderOutput.CreateFile(relativeDestPath, ImageTypeMapper.ByImageFormat(imageFormat).mimeType);
			using (stream)
			{
				imageRef.image.Save(stream, imageFormat);
				stream.Close();
			}
		}
	}
}
