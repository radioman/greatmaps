using System;
namespace MSR.CVE.BackMaker
{
	public class PointD
	{
		public double x;
		public double y;
		public PointD()
		{
		}
		public PointD(double x, double y)
		{
			this.x = x;
			this.y = y;
		}
		public override string ToString()
		{
			return string.Format("({0}, {1})", this.x, this.y);
		}
		public static PointD operator +(PointD p0, PointD p1)
		{
			return new PointD(p0.x + p1.x, p0.y + p1.y);
		}
		public static PointD operator -(PointD p0, PointD p1)
		{
			return new PointD(p0.x - p1.x, p0.y - p1.y);
		}
		public double Length()
		{
			return Math.Sqrt(this.x * this.x + this.y * this.y);
		}
	}
}
