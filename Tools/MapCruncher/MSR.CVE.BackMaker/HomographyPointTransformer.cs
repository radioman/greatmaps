using System;
namespace MSR.CVE.BackMaker
{
	public class HomographyPointTransformer : IPointTransformer
	{
		public override void doTransform(PointD p0, PointD p1)
		{
		}
		public override IPointTransformer getInversePointTransfomer()
		{
			throw new NotImplementedException();
		}
	}
}
