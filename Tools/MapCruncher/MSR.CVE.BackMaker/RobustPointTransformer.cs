using System;
namespace MSR.CVE.BackMaker
{
	public class RobustPointTransformer : IPointTransformer
	{
		private IPointTransformer flakyPointTransformer;
		private IPointTransformer backupApproximatePointTransformer;
		public RobustPointTransformer(IPointTransformer flakyPointTransformer, IPointTransformer backupApproximatePointTransformer)
		{
			this.flakyPointTransformer = flakyPointTransformer;
			this.backupApproximatePointTransformer = backupApproximatePointTransformer;
		}
		public override void doTransform(PointD p0, PointD p1, out bool isApproximate)
		{
			try
			{
				isApproximate = false;
				this.flakyPointTransformer.doTransform(p0, p1);
			}
			catch (TransformFailedException)
			{
				isApproximate = true;
				this.backupApproximatePointTransformer.doTransform(p0, p1);
			}
		}
		public override IPointTransformer getInversePointTransfomer()
		{
			throw new NotImplementedException();
		}
	}
}
