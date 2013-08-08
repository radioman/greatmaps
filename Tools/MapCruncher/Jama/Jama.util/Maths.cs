using System;
namespace Jama.util
{
	public class Maths
	{
		public static double hypot(double a, double b)
		{
			double num;
			if (Math.Abs(a) > Math.Abs(b))
			{
				num = b / a;
				num = Math.Abs(a) * Math.Sqrt(1.0 + num * num);
			}
			else
			{
				if (b != 0.0)
				{
					num = a / b;
					num = Math.Abs(b) * Math.Sqrt(1.0 + num * num);
				}
				else
				{
					num = 0.0;
				}
			}
			return num;
		}
	}
}
