using Jama;
using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class SlowGeneralPolyPointTransformer : IPolyPointTransformer
	{
		private JamaMatrix matrix;
		public SlowGeneralPolyPointTransformer(int polynomialDegree, JamaMatrix matrix) : base(matrix)
		{
			this.polynomialDegree = polynomialDegree;
			this.matrix = matrix;
		}
		public override void doTransform(PointD p0, PointD p1)
		{
			JamaMatrix jamaMatrix = new JamaMatrix(1, 2);
			jamaMatrix.SetElement(0, 0, p0.x);
			jamaMatrix.SetElement(0, 1, p0.y);
			JamaMatrix jamaMatrix2 = IPolyPointTransformer.Polynomialize(jamaMatrix, this.polynomialDegree).times(this.matrix);
			p1.x = jamaMatrix2.GetElement(0, 0);
			p1.y = jamaMatrix2.GetElement(1, 0);
		}
	}
}
