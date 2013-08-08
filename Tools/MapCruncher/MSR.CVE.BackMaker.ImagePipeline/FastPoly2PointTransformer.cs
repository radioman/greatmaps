using Jama;
using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	internal class FastPoly2PointTransformer : IPolyPointTransformer
	{
		private double[] c = new double[12];
		public FastPoly2PointTransformer(JamaMatrix matrix) : base(matrix)
		{
			this.polynomialDegree = 2;
			for (int i = 0; i < 12; i++)
			{
				this.c[i] = matrix.GetElement(i, 0);
			}
		}
		public override void doTransform(PointD p0, PointD p1)
		{
			double num = p0.x * p0.x;
			double num2 = p0.x * p0.y;
			double num3 = p0.y * p0.y;
			p1.x = this.c[0] + this.c[1] * p0.y + this.c[2] * num3 + this.c[3] * p0.x + this.c[4] * num2 + this.c[5] * num;
			p1.y = this.c[6] + this.c[7] * p0.y + this.c[8] * num3 + this.c[9] * p0.x + this.c[10] * num2 + this.c[11] * num;
		}
	}
}
