using System;
using System.Drawing.Imaging;
namespace MSR.CVE.BackMaker
{
	public class OutputTileType
	{
		private string _extn;
		private ImageFormat _imageFormat;
		public static OutputTileType PNG = new OutputTileType("png", ImageFormat.Png);
		public static OutputTileType JPG = new OutputTileType("jpg", ImageFormat.Jpeg);
		public static OutputTileType IPIC = new OutputTileType("ipic", null);
		public string extn
		{
			get
			{
				return this._extn;
			}
		}
		public ImageFormat imageFormat
		{
			get
			{
				D.Assert(this._imageFormat != null);
				return this._imageFormat;
			}
		}
		private OutputTileType(string extn, ImageFormat imageFormat)
		{
			this._extn = extn;
			this._imageFormat = imageFormat;
		}
		public static OutputTileType Parse(string extn)
		{
			if (extn == "png")
			{
				return OutputTileType.PNG;
			}
			if (extn == "jpg")
			{
				return OutputTileType.JPG;
			}
			if (extn == "ipic")
			{
				return OutputTileType.IPIC;
			}
			throw new UnknownImageTypeException(string.Format("Unrecognized output type {0}", extn));
		}
	}
}
