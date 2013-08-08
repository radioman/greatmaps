using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class GDIOpenDocument : VerbPresent, Verb, Present, IDisposable, SizedObject
	{
		private string sourceFilename;
		private GDIBigLockedImage loadedImage;
		private RectangleF actualBoundingBox;
		private RectangleF boundingBox;
		private int hackRectangleAdjust;
		public GDIOpenDocument(string sourceFilename)
		{
			this.sourceFilename = sourceFilename;
			this.loadedImage = GDIBigLockedImage.FromFile(sourceFilename);
			D.Sayf(0, "GDIOpenDocument Image.FromFile({0})", new object[]
			{
				sourceFilename
			});
			this.actualBoundingBox = new RectangleF(default(PointF), this.loadedImage.Size);
			this.boundingBox = SourceMapRendererTools.ToSquare(this.actualBoundingBox);
			if (sourceFilename.EndsWith("emf") || sourceFilename.EndsWith("wmf"))
			{
				this.hackRectangleAdjust = 1;
			}
		}
		public void AccumulateRobustHash(IRobustHash hash)
		{
			hash.Accumulate("GDIOpenDocument");
			hash.Accumulate(this.sourceFilename);
		}
		public Present Duplicate(string refCredit)
		{
			return this;
		}
		public void Dispose()
		{
			Monitor.Enter(this);
			try
			{
				this.loadedImage.Dispose();
				this.loadedImage = null;
			}
			finally
			{
				Monitor.Exit(this);
			}
			D.Sayf(0, "GDIOpenDocument Dispose({0})", new object[]
			{
				this.sourceFilename
			});
		}
		internal Present Render(MapRectangle mapRect, Size size, bool useDocumentTransparency, bool exactColors)
		{
			Monitor.Enter(this);
			Present result;
			try
			{
				RectangleD rectangleD = new RectangleD(mapRect.lon0 * (double)this.boundingBox.Width - 0.5, -mapRect.lat1 * (double)this.boundingBox.Height + (double)this.actualBoundingBox.Height - 0.5, (mapRect.lon1 - mapRect.lon0) * (double)this.boundingBox.Width + (double)this.hackRectangleAdjust, (mapRect.lat1 - mapRect.lat0) * (double)this.boundingBox.Height + (double)this.hackRectangleAdjust);
				RectangleD rectangleD2 = new RectangleD(0.0, 0.0, (double)size.Width, (double)size.Height);
				this.Reclip(this.actualBoundingBox, ref rectangleD, ref rectangleD2);
				D.Say(10, string.Format("Rendering {0} from {1}", mapRect, rectangleD));
				GDIBigLockedImage gDIBigLockedImage = new GDIBigLockedImage(size, "GDIVerb");
				if (exactColors)
				{
					gDIBigLockedImage.SetInterpolationMode(InterpolationMode.NearestNeighbor);
				}
				else
				{
					gDIBigLockedImage.SetInterpolationMode(InterpolationMode.HighQualityBicubic);
				}
				gDIBigLockedImage.DrawImageOntoThis(this.loadedImage, rectangleD2.ToRectangleF(), rectangleD.ToRectangleF());
				result = new ImageRef(new ImageRefCounted(gDIBigLockedImage));
			}
			finally
			{
				Monitor.Exit(this);
			}
			return result;
		}
		private void Reclip(RectangleF sourceBounds, ref RectangleD sourceRect, ref RectangleD outRect)
		{
			double num = sourceRect.X;
			double num2 = sourceRect.Right;
			double num3 = sourceRect.Y;
			double num4 = sourceRect.Bottom;
			double num5 = outRect.X;
			double num6 = outRect.Right;
			double num7 = outRect.Y;
			double num8 = outRect.Bottom;
			if (sourceRect.Right < (double)sourceBounds.Left || sourceRect.Right > (double)sourceBounds.Right)
			{
				if (sourceRect.Right < (double)sourceBounds.Left)
				{
					num2 = (double)sourceBounds.Left;
				}
				else
				{
					if (sourceRect.Right > (double)sourceBounds.Right)
					{
						num2 = (double)sourceBounds.Right;
					}
				}
				num6 = outRect.Left + outRect.Width * (num2 - sourceRect.Left) / sourceRect.Width;
			}
			if (sourceRect.Left < (double)sourceBounds.Left || sourceRect.Left > (double)sourceBounds.Right)
			{
				if (sourceRect.Left < (double)sourceBounds.Left)
				{
					num = (double)sourceBounds.Left;
				}
				else
				{
					if (sourceRect.Left > (double)sourceBounds.Right)
					{
						num = (double)sourceBounds.Right;
					}
				}
				num5 = outRect.Left + outRect.Width * (num - sourceRect.Left) / sourceRect.Width;
			}
			if (sourceRect.Top < (double)sourceBounds.Top || sourceRect.Top > (double)sourceBounds.Bottom)
			{
				if (sourceRect.Top < (double)sourceBounds.Top)
				{
					num3 = (double)sourceBounds.Top;
				}
				else
				{
					if (sourceRect.Top > (double)sourceBounds.Bottom)
					{
						num3 = (double)sourceBounds.Bottom;
					}
				}
				num7 = outRect.Top + outRect.Height * (num3 - sourceRect.Top) / sourceRect.Height;
			}
			if (sourceRect.Bottom < (double)sourceBounds.Top || sourceRect.Bottom > (double)sourceBounds.Bottom)
			{
				if (sourceRect.Bottom < (double)sourceBounds.Top)
				{
					num4 = (double)sourceBounds.Top;
				}
				else
				{
					if (sourceRect.Bottom > (double)sourceBounds.Bottom)
					{
						num4 = (double)sourceBounds.Bottom;
					}
				}
				num8 = outRect.Top + outRect.Height * (num4 - sourceRect.Top) / sourceRect.Height;
			}
			RectangleD rectangleD = new RectangleD(num, num3, num2 - num, num4 - num3);
			RectangleD rectangleD2 = new RectangleD(num5, num7, num6 - num5, num8 - num7);
			sourceRect = rectangleD;
			outRect = rectangleD2;
		}
		internal Present FetchBounds()
		{
			return new BoundsPresent(new RenderRegion(new MapRectangle(0.0, 0.0, (double)(this.actualBoundingBox.Height / this.boundingBox.Height), (double)(this.actualBoundingBox.Width / this.boundingBox.Width)), new DirtyEvent()));
		}
		public long GetSize()
		{
			return (long)(this.actualBoundingBox.Width * this.actualBoundingBox.Height * 4f);
		}
		internal Present ImageDetail(Size assumedDisplaySize)
		{
			double num = Math.Max((double)this.loadedImage.Width / (double)assumedDisplaySize.Width, (double)this.loadedImage.Height / (double)assumedDisplaySize.Height);
			num = Math.Max(num, 1.0);
			int num2 = 1 + (int)Math.Ceiling(Math.Log(num) / Math.Log(2.0));
			D.Assert(num2 >= 0);
			return new IntParameter(num2);
		}
		public Present Evaluate(Present[] paramList)
		{
			if (!(paramList[0] is IntParameter))
			{
				return PresentFailureCode.FailedCast(paramList[0], "FoxitOpenDocument.Evaluate");
			}
			switch (((IntParameter)paramList[0]).value)
			{
			case 0:
			{
				D.Assert(paramList.Length == 5);
				MapRectangleParameter mapRectangleParameter = (MapRectangleParameter)paramList[1];
				SizeParameter sizeParameter = (SizeParameter)paramList[2];
				BoolParameter boolParameter = (BoolParameter)paramList[3];
				BoolParameter boolParameter2 = (BoolParameter)paramList[4];
				return this.Render(mapRectangleParameter.value, sizeParameter.value, boolParameter.value, boolParameter2.value);
			}
			case 1:
				D.Assert(paramList.Length == 1);
				return this.FetchBounds();
			case 2:
			{
				D.Assert(paramList.Length == 2);
				SizeParameter sizeParameter2 = (SizeParameter)paramList[1];
				return this.ImageDetail(sizeParameter2.value);
			}
			case 3:
				D.Assert(paramList.Length == 1);
				return new StringParameter(null);
			default:
				return new PresentFailureCode("Invalid AccessVerb");
			}
		}
	}
}
