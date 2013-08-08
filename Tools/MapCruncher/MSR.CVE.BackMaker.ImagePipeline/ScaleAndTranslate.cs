using System;
using System.Windows.Media;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	internal class ScaleAndTranslate
	{
		private double scx;
		private double scy;
		private double tx;
		private double ty;
		public ScaleAndTranslate(RectangleD source, RectangleD dest)
		{
			this.scx = (dest.Right - dest.Left) / (source.Right - source.Left);
			this.tx = dest.Left - this.scx * source.Left;
			this.scy = (dest.Bottom - dest.Top) / (source.Bottom - source.Top);
			this.ty = dest.Top - this.scy * source.Top;
		}
		public ScaleAndTranslate(double tx, double ty)
		{
			this.scx = 1.0;
			this.scy = 1.0;
			this.tx = tx;
			this.ty = ty;
		}
		public RectangleD Apply(RectangleD a)
		{
			double num = this.scx * a.Left + this.tx;
			double num2 = this.scy * a.Top + this.ty;
			double num3 = this.scx * a.Right + this.tx;
			double num4 = this.scy * a.Bottom + this.ty;
			return new RectangleD(num, num2, num3 - num, num4 - num2);
		}
		public ScaleTransform ToScaleTransform()
		{
			return new ScaleTransform(this.scx, this.scy);
		}
		public override string ToString()
		{
			return string.Format("Scale({0},{1})Transform({2},{3})", new object[]
			{
				this.scx,
				this.scy,
				this.tx,
				this.ty
			});
		}
	}
}
