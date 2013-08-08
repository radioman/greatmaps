using System;
using System.Drawing.Imaging;
namespace MSR.CVE.BackMaker
{
	public class ImageTypeMapping
	{
		private string _extension;
		private string _mimeType;
		private ImageFormat _imageFormat;
		public string extension
		{
			get
			{
				return this._extension;
			}
		}
		public string mimeType
		{
			get
			{
				return this._mimeType;
			}
		}
		public ImageFormat imageFormat
		{
			get
			{
				return this._imageFormat;
			}
		}
		public ImageTypeMapping(string extension, string mimeType, ImageFormat imageFormat)
		{
			this._extension = extension;
			this._mimeType = mimeType;
			this._imageFormat = imageFormat;
		}
		public bool ImageFormatEquals(ImageFormat otherFormat)
		{
			return this._imageFormat != null && otherFormat != null && this._imageFormat.Guid == otherFormat.Guid;
		}
	}
}
