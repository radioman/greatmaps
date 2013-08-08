using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class FadeVerb : Verb
	{
		private FadeOptions fadeOptions;
		public FadeVerb(FadeOptions fadeOptions)
		{
			this.fadeOptions = new FadeOptions(fadeOptions);
		}
		public void AccumulateRobustHash(IRobustHash hash)
		{
			hash.Accumulate("FadeVerb(");
			this.fadeOptions.AccumulateRobustHash(hash);
			hash.Accumulate(")");
		}
		public Present Evaluate(Present[] paramList)
		{
			D.Assert(paramList.Length == 2);
			if (!(paramList[0] is ImageRef))
			{
				return paramList[0];
			}
			if (!(paramList[1] is TileAddress))
			{
				return paramList[1];
			}
			ImageRef imageRef = (ImageRef)paramList[0];
			TileAddress tileAddress = (TileAddress)paramList[1];
			double fadeForZoomLevel = this.fadeOptions.GetFadeForZoomLevel(tileAddress.ZoomLevel);
			if (fadeForZoomLevel == 1.0)
			{
				return imageRef;
			}
			return this.FadeTile(imageRef, fadeForZoomLevel);
		}
		private unsafe Present FadeTile(ImageRef sourceImage, double fadeFactor)
		{
            GDIBigLockedImage image = new GDIBigLockedImage(sourceImage.image.Size, "FadeVerb");
            lock (sourceImage.image)
            {
                Image image2 = sourceImage.image.IPromiseIAmHoldingGDISLockSoPleaseGiveMeTheImage();
                lock (image)
                {
                    Image image3 = image.IPromiseIAmHoldingGDISLockSoPleaseGiveMeTheImage();
                    Bitmap bitmap = (Bitmap)image2;
                    Bitmap bitmap2 = (Bitmap)image3;
                    BitmapData bitmapdata = bitmap2.LockBits(new Rectangle(0, 0, bitmap2.Width, bitmap2.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                    BitmapData data2 = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                    PixelStruct* structPtr = (PixelStruct*)bitmapdata.Scan0;
                    PixelStruct* structPtr3 = (PixelStruct*)data2.Scan0;
                    int width = bitmapdata.Width;
                    int height = bitmapdata.Height;
                    int num3 = bitmapdata.Stride / sizeof(PixelStruct);
                    for (int i = 0; i < height; i++)
                    {
                        int num4 = 0;
                        for (PixelStruct* structPtr2 = structPtr + (i * num3); num4 < width; structPtr2++)
                        {
                            PixelStruct* structPtr4 = (structPtr3 + (i * num3)) + num4;
                            structPtr2[0] = structPtr4[0];
                            structPtr2->a = (byte)(structPtr2->a * fadeFactor);
                            num4++;
                        }
                    }
                    bitmap2.UnlockBits(bitmapdata);
                    bitmap.UnlockBits(data2);
                }
            }
            sourceImage.Dispose();
            return new ImageRef(new ImageRefCounted(image));
		}
	}
}
