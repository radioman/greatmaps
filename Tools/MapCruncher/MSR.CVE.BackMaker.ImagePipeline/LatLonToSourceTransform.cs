using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class LatLonToSourceTransform : IPointTransformer
	{
		private IPointTransformer mercatorToSource;
		public LatLonToSourceTransform(IPointTransformer mercatorToSource)
		{
			this.mercatorToSource = mercatorToSource;
		}
		public override void doTransform(PointD p0, PointD p1)
		{
			PointD transformedPoint = this.mercatorToSource.getTransformedPoint(MercatorCoordinateSystem.LatLonToMercator(p0));
			p1.x = transformedPoint.x;
			p1.y = transformedPoint.y;
		}
		public override IPointTransformer getInversePointTransfomer()
		{
			throw new NotImplementedException();
		}
	}
}
