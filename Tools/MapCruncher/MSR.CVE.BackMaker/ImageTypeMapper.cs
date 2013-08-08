using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
namespace MSR.CVE.BackMaker
{
	public static class ImageTypeMapper
	{
		private static List<ImageTypeMapping> mappings = ImageTypeMapper.InitMappings();
		private static List<ImageTypeMapping> InitMappings()
		{
			return new List<ImageTypeMapping>
			{
				new ImageTypeMapping("png", "image/png", ImageFormat.Png),
				new ImageTypeMapping("jpg", "image/jpeg", ImageFormat.Jpeg),
				new ImageTypeMapping("jpeg", "image/jpeg", ImageFormat.Jpeg),
				new ImageTypeMapping("pdf", "application/pdf", null),
				new ImageTypeMapping("tif", "image/tiff", ImageFormat.Tiff),
				new ImageTypeMapping("tiff", "image/tiff", ImageFormat.Tiff),
				new ImageTypeMapping("wmf", "image/wmf", ImageFormat.Wmf),
				new ImageTypeMapping("emf", "image/x-emf", ImageFormat.Emf),
				new ImageTypeMapping("bmp", "image/bmp", ImageFormat.Bmp),
				new ImageTypeMapping("gif", "image/gif", ImageFormat.Gif)
			};
		}
		public static ImageTypeMapping ByExtension(string extension)
		{
			if (extension[0] == '.')
			{
				extension = extension.Substring(1);
			}
			extension = extension.ToLower();
			ImageTypeMapping imageTypeMapping = ImageTypeMapper.mappings.Find((ImageTypeMapping candidate) => candidate.extension == extension);
			if (imageTypeMapping == null)
			{
				throw new UnknownImageTypeException(extension);
			}
			return imageTypeMapping;
		}
		public static ImageTypeMapping ByMimeType(string mimeType)
		{
			ImageTypeMapping imageTypeMapping = ImageTypeMapper.mappings.Find((ImageTypeMapping candidate) => candidate.mimeType == mimeType);
			if (imageTypeMapping == null)
			{
				throw new UnknownImageTypeException(mimeType);
			}
			return imageTypeMapping;
		}
		public static ImageTypeMapping ByImageFormat(ImageFormat imageFormat)
		{
			ImageTypeMapping imageTypeMapping = ImageTypeMapper.mappings.Find((ImageTypeMapping candidate) => candidate.ImageFormatEquals(imageFormat));
			if (imageTypeMapping == null)
			{
				throw new UnknownImageTypeException(imageFormat.ToString());
			}
			return imageTypeMapping;
		}
	}
}
