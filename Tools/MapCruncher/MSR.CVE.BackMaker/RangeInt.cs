using System;
using System.Globalization;
using System.Runtime.CompilerServices;
namespace MSR.CVE.BackMaker
{
	public class RangeInt : Range<int>
	{
        //[CompilerGenerated]
        //private static Range<int>.Escape <>9__CachedAnonymousMethodDelegate1;

		public RangeInt(int min, int max) : base(min, max)
		{
		}
		protected override int Parse(string str)
		{
			return Convert.ToInt32(str, CultureInfo.InvariantCulture);
		}
		public int ParseAllowUndefinedZoom(MashupParseContext context, string fieldName, string str)
		{
			return base.Parse(context, fieldName, str, (int value) => value == -1);
		}
	}
}
