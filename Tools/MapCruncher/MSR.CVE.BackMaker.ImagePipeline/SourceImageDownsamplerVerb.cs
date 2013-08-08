using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	internal class SourceImageDownsamplerVerb : Verb
	{
		private const int memoizedDim = 1024;
		private IFuturePrototype prototype;
		private Size memoizedSize = new Size(1024, 1024);
		private MapRectangle unitRectangle = new MapRectangle(0.0, 0.0, 1.0, 1.0);
		public SourceImageDownsamplerVerb(IFuturePrototype prototype)
		{
			this.prototype = prototype;
		}
		public Present Evaluate(Present[] paramList)
		{
            D.Assert(paramList.Length == 4);
            MapRectangleParameter parameter = (MapRectangleParameter)paramList[0];
            SizeParameter parameter2 = (SizeParameter)paramList[1];
            Present present = paramList[2];
            Present present2 = paramList[3];
            double num = Math.Min(parameter.value.LonExtent, parameter.value.LatExtent);
            int num2 = Math.Max(parameter2.value.Width, parameter2.value.Height);
            if ((num > 1.0) && (num2 <= 0x400))
            {
                IFuture future = this.prototype.Curry(new ParamDict(new object[] { TermName.ImageBounds, new MapRectangleParameter(this.unitRectangle), TermName.OutputSize, new SizeParameter(this.memoizedSize), TermName.UseDocumentTransparency, present, TermName.ExactColors, present2 }));
                StrongHash hash = new StrongHash();
                future.AccumulateRobustHash(hash);
                D.Sayf(0, "Future {0} hashes to {1}", new object[] { RobustHashTools.DebugString(future), hash.ToString() });
                Present present3 = future.Realize("sourceImageDownsampler-memo");
                if (present3 is ImageRef)
                {
                    try
                    {
                        ImageRef ref2 = (ImageRef)present3;
                        GDIBigLockedImage image = new GDIBigLockedImage(parameter2.value, "sourceImageDownsampler-downsample");
                        lock (ref2.image)
                        {
                            lock (image)
                            {
                                Graphics graphics = image.IPromiseIAmHoldingGDISLockSoPleaseGiveMeTheGraphics();
                                MapRectangle region = parameter.value.Intersect(this.unitRectangle);
                                if (!region.IsEmpty())
                                {
                                    RectangleF srcRect = this.SelectSubRectangle(this.unitRectangle, region, this.memoizedSize);
                                    RectangleF destRect = this.SelectSubRectangle(parameter.value, region, parameter2.value);
                                    Image image2 = ref2.image.IPromiseIAmHoldingGDISLockSoPleaseGiveMeTheImage();
                                    graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;
                                    graphics.DrawImage(image2, destRect, srcRect, GraphicsUnit.Pixel);
                                }
                                graphics.Dispose();
                                return new ImageRef(new ImageRefCounted(image));
                            }
                        }
                    }
                    catch (ArgumentException)
                    {
                        return new PresentFailureCode("Image processing overflow");
                    }
                    catch (OverflowException)
                    {
                        return new PresentFailureCode("Image processing overflow");
                    }
                    catch (Exception exception)
                    {
                        return new PresentFailureCode(exception);
                    }
                    finally
                    {
                        present3.Dispose();
                    }
                }
                return present3;
            }
            return this.prototype.Curry(new ParamDict(new object[] { TermName.ImageBounds, parameter, TermName.OutputSize, parameter2, TermName.UseDocumentTransparency, present, TermName.ExactColors, present2 })).Realize("sourceImageDownsampler-passthru");
		}
		private RectangleF SelectSubRectangle(MapRectangle bounds, MapRectangle region, Size size)
		{
			float num = (float)((region.lon0 - bounds.lon0) / bounds.LonExtent * (double)size.Width);
			float num2 = (float)((double)size.Height - (region.lat0 - bounds.lat0) / bounds.LatExtent * (double)size.Height);
			float num3 = (float)((region.lon1 - bounds.lon0) / bounds.LonExtent * (double)size.Width);
			float num4 = (float)((double)size.Height - (region.lat1 - bounds.lat0) / bounds.LatExtent * (double)size.Height);
			return new RectangleF(num, num4, num3 - num, num2 - num4);
		}
		public void AccumulateRobustHash(IRobustHash hash)
		{
			hash.Accumulate("SourceImageDownsamplerVerb(");
			DummyTerm dummyTerm = new DummyTerm();
			IFuture future = this.prototype.Curry(new ParamDict(new object[]
			{
				TermName.ImageBounds,
				dummyTerm,
				TermName.OutputSize,
				dummyTerm,
				TermName.UseDocumentTransparency,
				dummyTerm,
				TermName.ExactColors,
				dummyTerm
			}));
			future.AccumulateRobustHash(hash);
			hash.Accumulate(")");
		}
	}
}
