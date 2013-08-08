using System;
using System.Drawing;
namespace MSR.CVE.BackMaker
{
	public class Point64
	{
		private long _x;
		private long _y;
		public long X
		{
			get
			{
				return this._x;
			}
			set
			{
				this._x = value;
			}
		}
		public long Y
		{
			get
			{
				return this._y;
			}
			set
			{
				this._y = value;
			}
		}
		public Point64(long x, long y)
		{
			this._x = x;
			this._y = y;
		}
		public Point ToPoint()
		{
			D.Assert(this._x <= 2147483647L && this._x >= -2147483648L && this._y <= 2147483647L && this._y >= -2147483648L);
			return new Point((int)this._x, (int)this._y);
		}
	}
}
