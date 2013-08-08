using System;
using System.Runtime.InteropServices;
namespace MSR.CVE.BackMaker
{
	public class FoxitLibWorker
	{
		[DllImport("fpdfview.dll")]
		private static extern void FPDF_UnlockDLL(string license_id, string unlock_code);
		[DllImport("fpdfview.dll")]
		private static extern int FPDF_LoadDocument(string file_path, string password);
		[DllImport("fpdfview.dll")]
		private static extern int FPDF_GetPageCount(int pdf_doc);
		[DllImport("fpdfview.dll")]
		private static extern int FPDF_LoadPage(int pdf_doc, int page_index);
		[DllImport("fpdfview.dll")]
		private static extern double FPDF_GetPageWidth(int pdf_page);
		[DllImport("fpdfview.dll")]
		private static extern double FPDF_GetPageHeight(int pdf_page);
		[DllImport("fpdfview.dll")]
		private static extern void FPDF_RenderPage(IntPtr hdc, int pdf_page, int start_x, int start_y, int size_x, int size_y, int rotate, int flags);
		[DllImport("fpdfview.dll")]
		private static extern int FPDF_ClosePage(int pdf_page);
		[DllImport("fpdfview.dll")]
		private static extern int FPDF_CloseDocument(int pdf_doc);
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
		public void UnlockDLL(string license_id, string unlock_code)
		{
			FoxitLibWorker.FPDF_UnlockDLL(license_id, unlock_code);
		}
		public int LoadDocument(string file_path, string password)
		{
			return FoxitLibWorker.FPDF_LoadDocument(file_path, password);
		}
		public int GetPageCount(int pdf_doc)
		{
			return FoxitLibWorker.FPDF_GetPageCount(pdf_doc);
		}
		public int LoadPage(int pdf_doc, int page_index)
		{
			return FoxitLibWorker.FPDF_LoadPage(pdf_doc, page_index);
		}
		public double GetPageWidth(int pdf_page)
		{
			return FoxitLibWorker.FPDF_GetPageWidth(pdf_page);
		}
		public double GetPageHeight(int pdf_page)
		{
			return FoxitLibWorker.FPDF_GetPageHeight(pdf_page);
		}
		public void RenderPage(IntPtr hdc, int pdf_page, int start_x, int start_y, int size_x, int size_y, int rotate, int flags)
		{
			FoxitLibWorker.FPDF_RenderPage(hdc, pdf_page, start_x, start_y, size_x, size_y, rotate, flags);
		}
		public int ClosePage(int pdf_page)
		{
			return FoxitLibWorker.FPDF_ClosePage(pdf_page);
		}
		public int CloseDocument(int pdf_doc)
		{
			return FoxitLibWorker.FPDF_CloseDocument(pdf_doc);
		}
		public int Bitmap_Create(int width, int height, int alpha)
		{
			return FoxitLibWorker.FPDFBitmap_Create(width, height, alpha);
		}
		public void RenderPageBitmap(int bitmap, int page, int start_x, int start_y, int size_x, int size_y, int rotate, int flags)
		{
			FoxitLibWorker.FPDF_RenderPageBitmap(bitmap, page, start_x, start_y, size_x, size_y, rotate, flags);
		}
		public void Bitmap_FillRect(int bitmap, int left, int top, int width, int height, int red, int green, int blue, int alpha)
		{
			FoxitLibWorker.FPDFBitmap_FillRect(bitmap, left, top, width, height, red, green, blue, alpha);
		}
		public IntPtr Bitmap_GetBuffer(int bitmap)
		{
			return FoxitLibWorker.FPDFBitmap_GetBuffer(bitmap);
		}
		public int Bitmap_GetWidth(int bitmap)
		{
			return FoxitLibWorker.FPDFBitmap_GetWidth(bitmap);
		}
		public int Bitmap_GetHeight(int bitmap)
		{
			return FoxitLibWorker.FPDFBitmap_GetHeight(bitmap);
		}
		public void Bitmap_Destroy(int bitmap)
		{
			FoxitLibWorker.FPDFBitmap_Destroy(bitmap);
		}
	}
}
