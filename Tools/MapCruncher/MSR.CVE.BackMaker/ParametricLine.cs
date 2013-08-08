using System;
using System.Collections.Generic;
namespace MSR.CVE.BackMaker
{
	public class ParametricLine
	{
		public class Intersection : IComparable
		{
			public bool IsParallel;
			public double t;
			public int CompareTo(object obj)
			{
				if (!(obj is ParametricLine.Intersection))
				{
					return 1;
				}
				ParametricLine.Intersection intersection = (ParametricLine.Intersection)obj;
				if (intersection.IsParallel && this.IsParallel)
				{
					return 0;
				}
				if (intersection.t == this.t)
				{
					return 0;
				}
				if (intersection.t > this.t)
				{
					return -1;
				}
				return 1;
			}
		}
		private LatLon s;
		private LatLon d;
		public ParametricLine(LatLon source, LatLon dest)
		{
			this.s = source;
			this.d = dest;
		}
		public LatLon t(double t_arg)
		{
			return new LatLon(this.s.lat + t_arg * (this.d.lat - this.s.lat), this.s.lon + t_arg * (this.d.lon - this.s.lon));
		}
		public double Length()
		{
			return Math.Sqrt(Math.Pow(this.d.lon - this.s.lon, 2.0) + Math.Pow(this.d.lat - this.s.lat, 2.0));
		}
		public ParametricLine.Intersection LatitudeIntersection(double lat)
		{
			ParametricLine.Intersection intersection = new ParametricLine.Intersection();
			if (this.s.lat == this.d.lat)
			{
				intersection.IsParallel = true;
			}
			else
			{
				intersection.IsParallel = false;
				intersection.t = (lat - this.s.lat) / (this.d.lat - this.s.lat);
			}
			return intersection;
		}
		public ParametricLine.Intersection LongitudeIntersection(double lon)
		{
			ParametricLine.Intersection intersection = new ParametricLine.Intersection();
			if (this.s.lon == this.d.lon)
			{
				intersection.IsParallel = true;
			}
			else
			{
				intersection.IsParallel = false;
				intersection.t = (lon - this.s.lon) / (this.d.lon - this.s.lon);
			}
			return intersection;
		}
		public List<LatLon> Interpolate(int NumSteps)
		{
			List<LatLon> list = new List<LatLon>();
			for (int i = 0; i < NumSteps; i++)
			{
				list.Add(this.t((double)i / (double)NumSteps));
			}
			return list;
		}
	}
}
