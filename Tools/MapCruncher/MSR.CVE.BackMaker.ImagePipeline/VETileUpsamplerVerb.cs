using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class VETileUpsamplerVerb : Verb
	{
		private IFuturePrototype veTileFetch;
		public VETileUpsamplerVerb(IFuturePrototype veTileFetch)
		{
			this.veTileFetch = veTileFetch;
		}
		public Present Evaluate(Present[] paramList)
		{
			TileAddress tileAddress = (TileAddress)paramList[0];
			if (tileAddress.ZoomLevel <= 19)
			{
				return this.veTileFetch.Curry(new ParamDict(new object[]
				{
					TermName.TileAddress,
					tileAddress
				})).Realize("VETileUpsamplerVerb");
			}
			int num = tileAddress.ZoomLevel - 19;
			int num2 = (1 << num) - 1;
			int num3 = tileAddress.TileX & num2;
			int num4 = tileAddress.TileY & num2;
			TileAddress tileAddress2 = new TileAddress(tileAddress.TileX >> num, tileAddress.TileY >> num, tileAddress.ZoomLevel - num);
			Size size = new Size(256, 256);
			Bitmap image = new Bitmap(size.Width * 3, size.Height * 3);
			Graphics graphics = Graphics.FromImage(image);
			float num5 = (float)(1 << num);
			RectangleF srcRect = new RectangleF((float)(num3 * size.Width) / num5 - 0.5f + (float)size.Width, (float)(num4 * size.Height) / num5 - 0.5f + (float)size.Height, (float)size.Width / num5, (float)size.Height / num5);
			for (int i = -1; i <= 1; i++)
			{
				bool flag = true;
				if ((i == -1 && srcRect.X >= (float)size.Width) || (i == 1 && srcRect.Right < (float)(2 * size.Width - 1)))
				{
					flag = false;
				}
				for (int j = -1; j <= 1; j++)
				{
					bool flag2 = true;
					if ((j == -1 && srcRect.Y >= (float)size.Height) || (j == 1 && srcRect.Bottom < (float)(2 * size.Height - 1)))
					{
						flag2 = false;
					}
					if (flag && flag2)
					{
						TileAddress tileAddress3 = new TileAddress(tileAddress2.TileX + i, tileAddress2.TileY + j, tileAddress2.ZoomLevel);
						Present present = this.veTileFetch.Curry(new ParamDict(new object[]
						{
							TermName.TileAddress,
							tileAddress3
						})).Realize("VETileUpsamplerVerb");
						try
						{
							if (present is ImageRef)
							{
								ImageRef imageRef = (ImageRef)present;
								GDIBigLockedImage image2;
								Monitor.Enter(image2 = imageRef.image);
								try
								{
									Rectangle srcRect2 = new Rectangle(0, 0, size.Width, size.Height);
									Rectangle destRect = new Rectangle(size.Width * (i + 1), size.Height * (j + 1), size.Width, size.Height);
									graphics.DrawImage(imageRef.image.IPromiseIAmHoldingGDISLockSoPleaseGiveMeTheImage(), destRect, srcRect2, GraphicsUnit.Pixel);
									goto IL_2B8;
								}
								finally
								{
									Monitor.Exit(image2);
								}
							}
							if (present is PresentFailureCode && i == 0 && j == 0)
							{
								return new PresentFailureCode((PresentFailureCode)present, "TileWebDownloader");
							}
							IL_2B8:;
						}
						finally
						{
							present.Dispose();
						}
					}
				}
			}
			Bitmap bitmap = new Bitmap(size.Width, size.Height);
			Graphics graphics2 = Graphics.FromImage(bitmap);
			graphics2.InterpolationMode = InterpolationMode.HighQualityBicubic;
			RectangleF destRect2 = new RectangleF(0f, 0f, (float)size.Width, (float)size.Height);
			graphics2.DrawImage(image, destRect2, srcRect, GraphicsUnit.Pixel);
			graphics2.Dispose();
			GDIBigLockedImage image3 = new GDIBigLockedImage(bitmap);
			return new ImageRef(new ImageRefCounted(image3));
		}
		public void AccumulateRobustHash(IRobustHash hash)
		{
			hash.Accumulate("VETileUpsamplerVerb(");
			this.veTileFetch.Curry(new ParamDict(new object[]
			{
				TermName.TileAddress,
				new DummyTerm()
			})).AccumulateRobustHash(hash);
			hash.Accumulate("(");
		}
	}
}
