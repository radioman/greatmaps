using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class TransparencyFuture : FutureBase
	{
		private TransparencyOptions transparencyOptions;
		private IFuture antialiasedFuture;
		private IFuture exactColorFuture;
		public TransparencyFuture(TransparencyOptions transparencyOptions, IFuture antialiasedFuture, IFuture exactColorFuture)
		{
			this.transparencyOptions = transparencyOptions;
			this.antialiasedFuture = antialiasedFuture;
			this.exactColorFuture = exactColorFuture;
		}
		public override Present Realize(string refCredit)
		{
			Present present = this.antialiasedFuture.Realize(refCredit);
			if (this.transparencyOptions.Effectless())
			{
				return present;
			}
			Present present2 = this.exactColorFuture.Realize(refCredit);
			return this.Evaluate(new Present[]
			{
				present,
				present2
			});
		}
		private unsafe Present Evaluate(params Present[] paramList)
		{
            D.Assert(paramList.Length == 2);
            if (!(paramList[0] is ImageRef))
            {
                return paramList[0];
            }
            if (!(paramList[1] is ImageRef))
            {
                return paramList[1];
            }
            ImageRef ref2 = (ImageRef)paramList[0];
            ImageRef ref3 = (ImageRef)paramList[1];
            GDIBigLockedImage image = new GDIBigLockedImage(ref2.image.Size, "TransparencyFuture");
            lock (ref2.image)
            {
                Image image2 = ref2.image.IPromiseIAmHoldingGDISLockSoPleaseGiveMeTheImage();
                lock (ref3.image)
                {
                    Image image3 = ref3.image.IPromiseIAmHoldingGDISLockSoPleaseGiveMeTheImage();
                    lock (image)
                    {
                        BitmapData data3;
                        Image image4 = image.IPromiseIAmHoldingGDISLockSoPleaseGiveMeTheImage();
                        Bitmap bitmap = (Bitmap)image2;
                        Bitmap bitmap2 = (Bitmap)image4;
                        Bitmap bitmap3 = (Bitmap)image3;
                        BitmapData bitmapdata = bitmap2.LockBits(new Rectangle(0, 0, bitmap2.Width, bitmap2.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                        BitmapData data2 = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                        if (bitmap3 == bitmap)
                        {
                            data3 = data2;
                        }
                        else
                        {
                            data3 = bitmap3.LockBits(new Rectangle(0, 0, bitmap3.Width, bitmap3.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                        }
                        PixelStruct* structPtr = (PixelStruct*)bitmapdata.Scan0;
                        PixelStruct* structPtr3 = (PixelStruct*)data2.Scan0;
                        PixelStruct* structPtr5 = (PixelStruct*)data3.Scan0;
                        int width = bitmapdata.Width;
                        int height = bitmapdata.Height;
                        int num3 = bitmapdata.Stride / sizeof(PixelStruct);
                        for (int i = 0; i < height; i++)
                        {
                            int num4 = 0;
                            for (PixelStruct* structPtr2 = structPtr + (i * num3); num4 < width; structPtr2++)
                            {
                                PixelStruct* structPtr4 = (structPtr3 + (i * num3)) + num4;
                                PixelStruct* structPtr6 = (structPtr5 + (i * num3)) + num4;
                                structPtr2[0] = structPtr4[0];
                                if (this.transparencyOptions.ShouldBeTransparent(structPtr6->r, structPtr6->g, structPtr6->b))
                                {
                                    structPtr2->a = 0;
                                }
                                num4++;
                            }
                        }
                        bitmap2.UnlockBits(bitmapdata);
                        bitmap.UnlockBits(data2);
                        if (bitmap3 != bitmap)
                        {
                            bitmap3.UnlockBits(data3);
                        }
                    }
                }
            }
            ref2.Dispose();
            ref3.Dispose();
            ImageRef source = new ImageRef(new ImageRefCounted(image));
            int num6 = 0;
            foreach (TransparencyColor color in this.transparencyOptions.colorList)
            {
                num6 = Math.Max(num6, color.halo);
            }
            if (num6 > 0)
            {
                HaloTransparency(source, num6);
            }
            return source;
		}
		public override void AccumulateRobustHash(IRobustHash hash)
		{
			hash.Accumulate("TransparencyVerb(");
			this.transparencyOptions.AccumulateRobustHash(hash);
			this.antialiasedFuture.AccumulateRobustHash(hash);
			this.exactColorFuture.AccumulateRobustHash(hash);
			hash.Accumulate(")");
		}
		public static void HaloTransparency(ImageRef source, int haloSize)
		{
			D.Assert(haloSize > 0 && haloSize < 100);
			ImageRef imageRef = source.Copy();
			GDIBigLockedImage image;
			Monitor.Enter(image = source.image);
			try
			{
				Image image2 = source.image.IPromiseIAmHoldingGDISLockSoPleaseGiveMeTheImage();
				for (int i = -haloSize; i < haloSize + 1; i++)
				{
					for (int j = -haloSize; j < haloSize + 1; j++)
					{
						if (i != 0 || j != 0)
						{
							Bitmap bitmap = new Bitmap(source.image.Width, source.image.Height);
							Graphics graphics = Graphics.FromImage(bitmap);
							Rectangle destRect = new Rectangle(0, 0, image2.Width, image2.Height);
							Rectangle srcRect = new Rectangle(i, j, image2.Width, image2.Height);
							graphics.DrawImage(image2, destRect, srcRect, GraphicsUnit.Pixel);
							graphics.Dispose();
							ImageRef imageRef2 = new ImageRef(new ImageRefCounted(new GDIBigLockedImage(bitmap)));
							TransparencyFuture.MaxInPlace(imageRef, imageRef2);
							imageRef2.Dispose();
						}
					}
				}
				TransparencyFuture.ReplaceAlphaChannel(source, imageRef);
			}
			finally
			{
				Monitor.Exit(image);
			}
			imageRef.Dispose();
		}
		public unsafe static void MaxInPlace(ImageRef target, ImageRef operand)
		{
            lock (target.image)
            {
                lock (operand.image)
                {
                    Image image = target.image.IPromiseIAmHoldingGDISLockSoPleaseGiveMeTheImage();
                    Image image2 = operand.image.IPromiseIAmHoldingGDISLockSoPleaseGiveMeTheImage();
                    Bitmap bitmap = (Bitmap)image;
                    Bitmap bitmap2 = (Bitmap)image2;
                    BitmapData bitmapdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                    BitmapData data2 = bitmap2.LockBits(new Rectangle(0, 0, bitmap2.Width, bitmap2.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                    PixelStruct* structPtr = (PixelStruct*)bitmapdata.Scan0;
                    PixelStruct* structPtr3 = (PixelStruct*)data2.Scan0;
                    int width = bitmapdata.Width;
                    int height = bitmapdata.Height;
                    int num3 = bitmapdata.Stride / sizeof(PixelStruct);
                    for (int i = 0; i < height; i++)
                    {
                        int num4 = 0;
                        PixelStruct* structPtr2 = structPtr + (i * num3);
                        for (PixelStruct* structPtr4 = structPtr3 + (i * num3); num4 < width; structPtr4++)
                        {
                            structPtr2[0] = PixelMax(structPtr2[0], structPtr4[0]);
                            num4++;
                            structPtr2++;
                        }
                    }
                    bitmap.UnlockBits(bitmapdata);
                    bitmap2.UnlockBits(data2);
                }
            }
		}
		public static PixelStruct PixelMax(PixelStruct pa, PixelStruct pb)
		{
			return new PixelStruct
			{
				r = Math.Max(pa.r, pb.r),
				g = Math.Max(pa.g, pb.g),
				b = Math.Max(pa.b, pb.b),
				a = Math.Max(pa.a, pb.a)
			};
		}
		public unsafe static void ReplaceAlphaChannel(ImageRef target, ImageRef alphaOperand)
		{
            lock (target.image)
            {
                lock (alphaOperand.image)
                {
                    Image image = target.image.IPromiseIAmHoldingGDISLockSoPleaseGiveMeTheImage();
                    Image image2 = alphaOperand.image.IPromiseIAmHoldingGDISLockSoPleaseGiveMeTheImage();
                    Bitmap bitmap = (Bitmap)image;
                    Bitmap bitmap2 = (Bitmap)image2;
                    BitmapData bitmapdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                    BitmapData data2 = bitmap2.LockBits(new Rectangle(0, 0, bitmap2.Width, bitmap2.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                    PixelStruct* structPtr = (PixelStruct*)bitmapdata.Scan0;
                    PixelStruct* structPtr3 = (PixelStruct*)data2.Scan0;
                    int width = bitmapdata.Width;
                    int height = bitmapdata.Height;
                    int num3 = bitmapdata.Stride / sizeof(PixelStruct);
                    for (int i = 0; i < height; i++)
                    {
                        int num4 = 0;
                        PixelStruct* structPtr2 = structPtr + (i * num3);
                        for (PixelStruct* structPtr4 = structPtr3 + (i * num3); num4 < width; structPtr4++)
                        {
                            structPtr2->a = structPtr4->a;
                            num4++;
                            structPtr2++;
                        }
                    }
                    bitmap.UnlockBits(bitmapdata);
                    bitmap2.UnlockBits(data2);
                }
            }
		}
	}
}
