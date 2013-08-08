using MSR.CVE.BackMaker.ImagePipeline;
using System;
using System.Drawing;
namespace MSR.CVE.BackMaker
{
	public struct PixelStruct
	{
		public byte b;
		public byte g;
		public byte r;
		public byte a;
		public static PixelStruct black()
		{
			PixelStruct result;
			result.b = 0;
			result.g = 0;
			result.r = 0;
			result.a = 0;
			return result;
		}
		public void AccumulateRobustHash(IRobustHash hash)
		{
			hash.Accumulate((int)this.b);
			hash.Accumulate((int)this.g);
			hash.Accumulate((int)this.r);
			hash.Accumulate((int)this.a);
		}
		public Color ToColor()
		{
			return Color.FromArgb((int)this.a, (int)this.r, (int)this.g, (int)this.b);
		}
		public static bool operator ==(PixelStruct p1, PixelStruct p2)
		{
			return p1.a == p2.a && p1.r == p2.r && p1.g == p2.g && p1.b == p2.b;
		}
		public static bool operator !=(PixelStruct p1, PixelStruct p2)
		{
			return !(p1 == p2);
		}
		public override bool Equals(object obj)
		{
			return obj is PixelStruct && this == (PixelStruct)obj;
		}
		public override int GetHashCode()
		{
			return (int)(this.a + 131 * (this.r + 131 * (this.g + 131 * this.b)));
		}
	}
}
