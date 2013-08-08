using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class SourceToLatLonTransform : IPointTransformer
	{
		private IPointTransformer sourceToMercator;
		public SourceToLatLonTransform(IPointTransformer sourceToMercator)
		{
			this.sourceToMercator = sourceToMercator;
		}
		public override void doTransform(PointD p0, PointD p1, out bool isApproximate)
		{
			PointD pointD = MercatorCoordinateSystem.MercatorToLatLon(this.sourceToMercator.getTransformedPoint(p0, out isApproximate));
			p1.x = pointD.x;
			p1.y = pointD.y;
		}
		public override IPointTransformer getInversePointTransfomer()
		{
			throw new NotImplementedException();
		}
	}
}
