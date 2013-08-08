using Jama;
using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class Affine2DPointTransformer : IPointTransformer
	{
		private double c0;
		private double c1;
		private double c2;
		private double c3;
		private double c4;
		private double c5;
		public Affine2DPointTransformer(JamaMatrix matrix)
		{
			this.c0 = matrix.GetElement(0, 0);
			this.c1 = matrix.GetElement(0, 1);
			this.c2 = matrix.GetElement(0, 2);
			this.c3 = matrix.GetElement(1, 0);
			this.c4 = matrix.GetElement(1, 1);
			this.c5 = matrix.GetElement(1, 2);
		}
		public override void doTransform(PointD p0, PointD p1)
		{
			p1.x = this.c0 * p0.x + this.c1 * p0.y + this.c2;
			p1.y = this.c3 * p0.x + this.c4 * p0.y + this.c5;
		}
		public override IPointTransformer getInversePointTransfomer()
		{
			throw new NotImplementedException();
		}
	}
}
