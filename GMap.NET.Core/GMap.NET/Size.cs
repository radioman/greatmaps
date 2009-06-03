
namespace GMap.NET
{
   using System.Globalization;

   /// <summary>
   /// the size
   /// </summary>
   public struct Size
   {
      public static readonly Size Empty = new Size();

      private int width;
      private int height;

      public Size(Point pt)
      {
         width = pt.X;
         height = pt.Y;
      }

      public Size(int width, int height)
      {
         this.width = width;
         this.height = height;
      }

      public static Size operator+(Size sz1, Size sz2)
      {
         return Add(sz1, sz2);
      }

      public static Size operator-(Size sz1, Size sz2)
      {
         return Subtract(sz1, sz2);
      }

      public static bool operator==(Size sz1, Size sz2)
      {
         return sz1.Width == sz2.Width && sz1.Height == sz2.Height;
      }

      public static bool operator!=(Size sz1, Size sz2)
      {
         return !(sz1 == sz2);
      }

      public static explicit operator Point(Size size)
      {
         return new Point(size.Width, size.Height);
      }

      public bool IsEmpty
      {
         get
         {
            return width == 0 && height == 0;
         }
      }

      public int Width
      {
         get
         {
            return width;
         }
         set
         {
            width = value;
         }
      }

      public int Height
      {
         get
         {
            return height;
         }
         set
         {
            height = value;
         }
      }

      public static Size Add(Size sz1, Size sz2)
      {
         return new Size(sz1.Width + sz2.Width, sz1.Height + sz2.Height);
      }

      public static Size Subtract(Size sz1, Size sz2)
      {
         return new Size(sz1.Width - sz2.Width, sz1.Height - sz2.Height);
      }

      public override bool Equals(object obj)
      {
         if(!(obj is Size))
            return false;

         Size comp = (Size) obj;
         // Note value types can't have derived classes, so we don't need to
         //
         return (comp.width == this.width) &&
                   (comp.height == this.height);
      }

      public override int GetHashCode()
      {
         return width ^ height;
      }

      public override string ToString()
      {
         return "{Width=" + width.ToString(CultureInfo.CurrentCulture) + ", Height=" + height.ToString(CultureInfo.CurrentCulture) + "}";
      }
   }
}
