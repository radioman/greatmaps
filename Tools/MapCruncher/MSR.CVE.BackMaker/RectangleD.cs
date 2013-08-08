using System;
using System.Drawing;
using System.Windows;
namespace MSR.CVE.BackMaker
{
	public class RectangleD
	{
		private double _x;
		private double _y;
		private double _width;
		private double _height;
		public double X
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
		public double Y
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
		public double Width
		{
			get
			{
				return this._width;
			}
			set
			{
				this._width = value;
			}
		}
		public double Height
		{
			get
			{
				return this._height;
			}
			set
			{
				this._height = value;
			}
		}
		public double Left
		{
			get
			{
				return this._x;
			}
		}
		public double Right
		{
			get
			{
				return this._x + this._width;
			}
		}
		public double Top
		{
			get
			{
				return this._y;
			}
		}
		public double Bottom
		{
			get
			{
				return this._y + this._height;
			}
		}
		public RectangleD(double x, double y, double width, double height)
		{
			this._x = x;
			this._y = y;
			this._width = width;
			this._height = height;
		}
		public RectangleF ToRectangleF()
		{
			return new RectangleF((float)this.X, (float)this.Y, (float)this.Width, (float)this.Height);
		}
		public RectangleD Round()
		{
			int num = (int)Math.Round(this.Left);
			int num2 = (int)Math.Round(this.Top);
			int num3 = (int)Math.Round(this.Right);
			int num4 = (int)Math.Round(this.Bottom);
			return new RectangleD((double)num, (double)num2, (double)(num3 - num), (double)(num4 - num2));
		}
		public Int32Rect ToInt32Rect()
		{
			int num = (int)Math.Round(this.Left);
			int num2 = (int)Math.Round(this.Top);
			int num3 = (int)Math.Round(this.Right);
			int num4 = (int)Math.Round(this.Bottom);
			return new Int32Rect(num, num2, num3 - num, num4 - num2);
		}
		public override string ToString()
		{
			return string.Format("RectangleD(x{0}, y{1}, w{2}, h{3})", new object[]
			{
				this._x,
				this._y,
				this._width,
				this._height
			});
		}
		public RectangleD Intersect(RectangleD r1)
		{
			double num = Math.Max(this.Left, r1.Left);
			double num2 = Math.Min(this.Right, r1.Right);
			double num3 = Math.Max(this.Top, r1.Top);
			double num4 = Math.Min(this.Bottom, r1.Bottom);
			return new RectangleD(num, num3, num2 - num, num4 - num3);
		}
		public RectangleD Grow(double margin)
		{
			return new RectangleD(this.X - margin, this.Y - margin, this.Width + 2.0 * margin, this.Height + 2.0 * margin);
		}
		internal bool IntIsEmpty()
		{
			Int32Rect int32Rect = this.ToInt32Rect();
			return int32Rect.Width <= 0 || int32Rect.Height <= 0;
		}
	}
}
