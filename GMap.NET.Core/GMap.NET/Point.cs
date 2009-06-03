
namespace GMap.NET
{
   using System.Globalization;

   /// <summary>
   /// the point ;}
   /// </summary>
   public struct Point
   {
      public static readonly Point Empty = new Point();

      private int x;
      private int y;

      public Point(int x, int y)
      {
         this.x = x;
         this.y = y;
      }

      public Point(Size sz)
      {
         this.x = sz.Width;
         this.y = sz.Height;
      }

      public Point(int dw)
      {
         this.x = (short) LOWORD(dw);
         this.y = (short) HIWORD(dw);
      }

      public bool IsEmpty
      {
         get
         {
            return x == 0 && y == 0;
         }
      }

      public int X
      {
         get
         {
            return x;
         }
         set
         {
            x = value;
         }
      }

      public int Y
      {
         get
         {
            return y;
         }
         set
         {
            y = value;
         }
      }

      public static explicit operator Size(Point p)
      {
         return new Size(p.X, p.Y);
      }

      public static Point operator+(Point pt, Size sz)
      {
         return Add(pt, sz);
      }

      public static Point operator-(Point pt, Size sz)
      {
         return Subtract(pt, sz);
      }

      public static bool operator==(Point left, Point right)
      {
         return left.X == right.X && left.Y == right.Y;
      }

      public static bool operator!=(Point left, Point right)
      {
         return !(left == right);
      }

      public static Point Add(Point pt, Size sz)
      {
         return new Point(pt.X + sz.Width, pt.Y + sz.Height);
      }

      public static Point Subtract(Point pt, Size sz)
      {
         return new Point(pt.X - sz.Width, pt.Y - sz.Height);
      }

      public override bool Equals(object obj)
      {
         if(!(obj is Point))
            return false;
         Point comp = (Point) obj;
         return comp.X == this.X && comp.Y == this.Y;
      }

      public override int GetHashCode()
      {
         return x ^ y;
      }

      public void Offset(int dx, int dy)
      {
         X += dx;
         Y += dy;
      }

      public void Offset(Point p)
      {
         Offset(p.X, p.Y);
      }

      public override string ToString()
      {
         return "{X=" + X.ToString(CultureInfo.CurrentCulture) + ",Y=" + Y.ToString(CultureInfo.CurrentCulture) + "}";
      }

      private static int HIWORD(int n)
      {
         return (n >> 16) & 0xffff;
      }

      private static int LOWORD(int n)
      {
         return n & 0xffff;
      }
   }
}
