using System;
namespace MSR.CVE.BackMaker
{
	internal class DownhillInverterPointTransformer : IPointTransformer
	{
		private const double thresh = 1E-07;
		private const int loopLimit = 200;
		private IPointTransformer destToSourceWarp;
		private IPointTransformer approximateSourceToDestWarp;
		public DownhillInverterPointTransformer(IPointTransformer destToSourceWarp, IPointTransformer approximateSourceToDestWarp)
		{
			this.destToSourceWarp = destToSourceWarp;
			this.approximateSourceToDestWarp = approximateSourceToDestWarp;
		}
		public override void doTransform(PointD p0, PointD p1)
		{
			try
			{
				PointD transformedPoint = this.getTransformedPoint(p0, false);
				p1.x = transformedPoint.x;
				p1.y = transformedPoint.y;
			}
			catch (TransformFailedException)
			{
				throw;
			}
		}
		protected PointD getTransformedPoint(PointD goal, bool debug)
		{
			PointD pointD = this.approximateSourceToDestWarp.getTransformedPoint(goal);
			PointD transformedPoint = this.destToSourceWarp.getTransformedPoint(pointD);
			PointD transformedPoint2 = this.approximateSourceToDestWarp.getTransformedPoint(transformedPoint);
			double num = (transformedPoint2 - pointD).Length();
			for (int i = 0; i < 200; i++)
			{
				PointD[] array = new PointD[]
				{
					pointD,
					new PointD(pointD.x + num, pointD.y),
					new PointD(pointD.x - num, pointD.y),
					new PointD(pointD.x, pointD.y + num),
					new PointD(pointD.x, pointD.y - num),
					new PointD(pointD.x + num * 0.707106781186547, pointD.y + num * 0.707106781186547),
					new PointD(pointD.x - num * 0.707106781186547, pointD.y + num * 0.707106781186547),
					new PointD(pointD.x - num * 0.707106781186547, pointD.y - num * 0.707106781186547),
					new PointD(pointD.x + num * 0.707106781186547, pointD.y - num * 0.707106781186547)
				};
				PointD[] array2 = Array.ConvertAll<PointD, PointD>(array, delegate(PointD p1)
				{
					PointD pointD2 = new PointD();
					this.destToSourceWarp.doTransform(p1, pointD2);
					return pointD2;
				});
				double[] array3 = new double[9];
				for (int j = 0; j < 9; j++)
				{
					array3[j] = (array2[j] - goal).Length();
				}
				int num2 = 0;
				for (int k = 1; k < 9; k++)
				{
					if (array3[k] < array3[num2])
					{
						num2 = k;
					}
				}
				double num3 = (array2[1] - array2[0]).Length();
				if (debug)
				{
					D.Say(0, string.Format("guess[{0:G19}] = {1} [{5}] source={6} dist = {2} radius={3} spread={4}", new object[]
					{
						num2,
						pointD,
						array3[num2],
						num,
						num3,
						num2,
						array2[num2]
					}));
				}
				double num4 = num * array3[num2] / num3;
				pointD = array[num2];
				double arg_25C_0 = array3[num2];
				if (num3 == 0.0 || (num4 >= num && num2 == 0))
				{
					D.Say(6, string.Format("Convergence iters: {0}", i));
					return pointD;
				}
				num = num4;
			}
			throw new TransformFailedException();
		}
		public override IPointTransformer getInversePointTransfomer()
		{
			throw new NotImplementedException();
		}
	}
}
