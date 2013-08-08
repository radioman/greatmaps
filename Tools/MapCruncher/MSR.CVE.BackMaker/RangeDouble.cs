using System;
using System.Globalization;
namespace MSR.CVE.BackMaker
{
	public class RangeDouble : Range<double>
	{
		public RangeDouble(double min, double max) : base(min, max)
		{
		}
		protected override double Parse(string str)
		{
			return Convert.ToDouble(str, CultureInfo.InvariantCulture);
		}
	}
}
