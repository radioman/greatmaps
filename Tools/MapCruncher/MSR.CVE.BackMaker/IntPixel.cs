using System;
namespace MSR.CVE.BackMaker
{
	public struct IntPixel
	{
		public int b;
		public int g;
		public int r;
		public int a;
		public static IntPixel BlackPixel()
		{
			IntPixel result;
			result.b = 0;
			result.g = 0;
			result.r = 0;
			result.a = 0;
			return result;
		}
		public void addWeighted(double weight, PixelStruct pix)
		{
			this.r += (int)(weight * (double)pix.r);
			this.g += (int)(weight * (double)pix.g);
			this.b += (int)(weight * (double)pix.b);
			this.a += (int)(weight * (double)pix.a);
		}
		public PixelStruct AsPixel()
		{
            PixelStruct struct2;
            struct2.r = (this.r > 0xff) ? ((byte)0xff) : ((byte)this.r);
            struct2.g = (this.g > 0xff) ? ((byte)0xff) : ((byte)this.g);
            struct2.b = (this.b > 0xff) ? ((byte)0xff) : ((byte)this.b);
            struct2.a = (this.a > 0xff) ? ((byte)0xff) : ((byte)this.a);
            return struct2;            
        }
	}
}
