using Jama;
using System;
namespace MSR.CVE.BackMaker
{
	internal class ElementwiseMath
	{
		public static double Min(JamaMatrix gm)
		{
			bool flag = true;
			double num = 0.0;
			if (gm.RowDimension * gm.ColumnDimension <= 0)
			{
				throw new Exception("No elements.");
			}
			for (int i = 0; i < gm.RowDimension; i++)
			{
				for (int j = 0; j < gm.ColumnDimension; j++)
				{
					double element = gm.GetElement(i, j);
					if (flag || element < num)
					{
						num = element;
						flag = false;
					}
				}
			}
			return num;
		}
		public static double Max(JamaMatrix gm)
		{
			bool flag = true;
			double num = 0.0;
			if (gm.RowDimension * gm.ColumnDimension <= 0)
			{
				throw new Exception("No elements.");
			}
			for (int i = 0; i < gm.RowDimension; i++)
			{
				for (int j = 0; j < gm.ColumnDimension; j++)
				{
					double element = gm.GetElement(i, j);
					if (flag || element > num)
					{
						num = element;
						flag = false;
					}
				}
			}
			return num;
		}
	}
}
