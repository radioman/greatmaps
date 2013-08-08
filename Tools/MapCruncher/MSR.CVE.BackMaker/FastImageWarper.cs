using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Threading;
namespace MSR.CVE.BackMaker
{
	internal class FastImageWarper
	{
		public unsafe static void doWarp(GDIBigLockedImage destImage, GDIBigLockedImage sourceImage, IPointTransformer[] pointTransformers, InterpolationMode mode)
		{
            Bitmap bitmap;
            BitmapData data;
            Bitmap bitmap2;
            BitmapData data2;
            DateTime now = DateTime.Now;
            PointD td = new PointD();
            PointD td2 = new PointD();
            lock (destImage)
            {
                bitmap = (Bitmap)destImage.IPromiseIAmHoldingGDISLockSoPleaseGiveMeTheImage();
                data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            }
            lock (sourceImage)
            {
                bitmap2 = (Bitmap)sourceImage.IPromiseIAmHoldingGDISLockSoPleaseGiveMeTheImage();
                data2 = bitmap2.LockBits(new Rectangle(0, 0, bitmap2.Width, bitmap2.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            }
            PixelStruct* structPtr = (PixelStruct*)data.Scan0;
            int width = data.Width;
            int height = data.Height;
            int num9 = data.Stride / sizeof(PixelStruct);
            PixelStruct* structPtr3 = (PixelStruct*)data2.Scan0;
            int num10 = data2.Width;
            int num11 = data2.Height;
            int num12 = data2.Stride / sizeof(PixelStruct);
            int num15 = num11 - 1;
            for (int i = 0; i < height; i++)
            {
                int num13 = 0;
                for (PixelStruct* structPtr2 = structPtr + (((height - 1) - i) * num9); num13 < width; structPtr2++)
                {
                    int x;
                    int num2;
                    PixelStruct* structPtr4;
                    td.x = num13;
                    td.y = i;
                    for (int j = 0; j < pointTransformers.Length; j++)
                    {
                        pointTransformers[j].doTransform(td, td2);
                        td.x = td2.x;
                        td.y = td2.y;
                    }
                    if (mode == InterpolationMode.NearestNeighbor)
                    {
                        x = (int)td.x;
                        num2 = num15 - ((int)td.y);
                        if (((x >= 0) && (x < num10)) && ((num2 >= 0) && (num2 < num11)))
                        {
                            structPtr4 = (structPtr3 + (num2 * num12)) + x;
                            structPtr2[0] = structPtr4[0];
                        }
                    }
                    else
                    {
                        if (mode != InterpolationMode.Bilinear)
                        {
                            throw new Exception("Unimplemented mode");
                        }
                        double num17 = num15 - td.y;
                        int num3 = (int)td.x;
                        int num4 = (int)num17;
                        double num5 = td.x - num3;
                        double num6 = num17 - num4;
                        IntPixel pixel = IntPixel.BlackPixel();
                        int num20 = 0;
                        for (double k = 1.0 - num5; num20 <= 1; k = num5)
                        {
                            int num21 = 0;
                            for (double m = 1.0 - num6; num21 <= 1; m = num6)
                            {
                                x = num3 + num20;
                                num2 = num4 + num21;
                                if (((x >= 0) && (x < num10)) && ((num2 >= 0) && (num2 < num11)))
                                {
                                    structPtr4 = (structPtr3 + (num2 * num12)) + x;
                                    pixel.addWeighted(k * m, structPtr4[0]);
                                }
                                num21++;
                            }
                            num20++;
                        }
                        structPtr2[0] = pixel.AsPixel();
                    }
                    num13++;
                }
            }
            lock (destImage)
            {
                bitmap.UnlockBits(data);
            }
            lock (sourceImage)
            {
                bitmap2.UnlockBits(data2);
            }
		}
	}
}
