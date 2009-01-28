using System;
using System.Runtime.InteropServices;

namespace GMapNET.Internals
{
   public class NativeMethods
   {
      [DllImport("GDI32.dll")]
      public static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);

      [DllImport("GDI32.dll")]
      public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

      [DllImport("GDI32.dll")]
      public static extern bool DeleteDC(IntPtr hdc);

      [DllImport("GDI32.dll")]
      public static extern bool DeleteObject(IntPtr hObject);

      [DllImport("gdi32.dll", ExactSpelling=true, PreserveSig=true, SetLastError=true)]
      public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

      [DllImport("Msimg32.dll")]
      public static extern bool TransparentBlt(IntPtr hdcDest, // handle to destination DC
      int nXOriginDest, // x-coord of destination upper-left corner
      int nYOriginDest, // y-coord of destination upper-left corner
      int nWidthDest, // width of destination rectangle
      int hHeightDest, // height of destination rectangle
      IntPtr hdcSrc, // handle to source DC
      int nXOriginSrc, // x-coord of source upper-left corner
      int nYOriginSrc, // y-coord of source upper-left corner
      int nWidthSrc, // width of source rectangle
      int nHeightSrc, // height of source rectangle
      int crTransparent // color to make transparent
      );
   }
}
