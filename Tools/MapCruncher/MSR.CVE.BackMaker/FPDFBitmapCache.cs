using System;
using System.Drawing;
using System.Runtime.InteropServices;
namespace MSR.CVE.BackMaker
{
	internal class FPDFBitmapCache
	{
		private bool created;
		private Size allocatedSize;
		private bool locked;
		private int bitmapHandle;
		[DllImport("fpdfview.dll")]
		private static extern int FPDFBitmap_Create(int width, int height, int alpha);
		[DllImport("fpdfview.dll")]
		private static extern void FPDF_RenderPageBitmap(int bitmap, int page, int start_x, int start_y, int size_x, int size_y, int rotate, int flags);
		[DllImport("fpdfview.dll")]
		private static extern void FPDFBitmap_FillRect(int bitmap, int left, int top, int width, int height, int red, int green, int blue, int alpha);
		[DllImport("fpdfview.dll")]
		private static extern IntPtr FPDFBitmap_GetBuffer(int bitmap);
		[DllImport("fpdfview.dll")]
		private static extern int FPDFBitmap_GetWidth(int bitmap);
		[DllImport("fpdfview.dll")]
		private static extern int FPDFBitmap_GetHeight(int bitmap);
		[DllImport("fpdfview.dll")]
		private static extern void FPDFBitmap_Destroy(int bitmap);
		public int Get(int width, int height)
		{
			D.Assert(!this.locked);
			if (!this.created || width != this.allocatedSize.Width || height != this.allocatedSize.Height)
			{
				this.dispose();
				this.allocatedSize = new Size(width, height);
				this.create();
			}
			D.Assert(this.created && width == this.allocatedSize.Width && height == this.allocatedSize.Height);
			this.locked = true;
			FPDFBitmapCache.FPDFBitmap_FillRect(this.bitmapHandle, 0, 0, this.allocatedSize.Width, this.allocatedSize.Height, 255, 255, 255, 255);
			return this.bitmapHandle;
		}
		private void dispose()
		{
			D.Assert(!this.locked);
			if (this.created)
			{
				FPDFBitmapCache.FPDFBitmap_Destroy(this.bitmapHandle);
				this.created = false;
			}
		}
		private void create()
		{
			D.Assert(!this.created);
			D.Assert(!this.locked);
			this.bitmapHandle = FPDFBitmapCache.FPDFBitmap_Create(this.allocatedSize.Width, this.allocatedSize.Height, 1);
			this.created = true;
		}
		public void Release(int bitmapHandle)
		{
			D.Assert(this.locked);
			this.locked = false;
		}
	}
}
