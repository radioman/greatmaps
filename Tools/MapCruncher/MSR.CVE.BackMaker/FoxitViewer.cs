using MSR.CVE.BackMaker.MCDebug;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
namespace MSR.CVE.BackMaker
{
	public class FoxitViewer : IFoxitViewer, IDisposable
	{
		private enum FoxitInvocationMode
		{
			UsingAppGraphicsContext,
			UsingFoxitBitmap,
			ReusingFoxitBitmap,
			UsingLocalGraphicsContext
		}
		private int docHandle;
		private int pageHandle;
		private int numPages;
		private FoxitLibWorker foxitLib = FoxitLibManager.theInstance.foxitLib;
		private string filename;
		internal static object FoxitMutex = new object();
		private static ResourceCounter foxitResourceCounter = DiagnosticUI.theDiagnostics.fetchResourceCounter("foxit", 5);
		public FoxitViewer(string filename, int pageNumber)
		{
			object foxitMutex;
			Monitor.Enter(foxitMutex = FoxitViewer.FoxitMutex);
			try
			{
				if (this.foxitLib == null)
				{
					throw FoxitLibManager.theInstance.loadException;
				}
				this.filename = filename;
				this.foxitLib.UnlockDLL("SDKRDYX1645", "3A6DE5356500929A15F3E6517416F91635E336C0");
				if ((this.docHandle = this.foxitLib.LoadDocument(filename, null)) == 0)
				{
					throw new Exception("Can't open " + filename);
				}
				this.numPages = this.foxitLib.GetPageCount(this.docHandle);
				if ((this.pageHandle = this.foxitLib.LoadPage(this.docHandle, pageNumber)) == 0)
				{
					this.foxitLib.CloseDocument(this.docHandle);
					throw new Exception("Can't open first page of " + filename);
				}
				FIBR.Announce("FoxitViewer.FoxitViewer", new object[]
				{
					MakeObjectID.Maker.make(this),
					filename,
					pageNumber
				});
				FoxitViewer.foxitResourceCounter.crement(1);
			}
			finally
			{
				Monitor.Exit(foxitMutex);
			}
		}
		public RectangleF GetPageSize()
		{
			object foxitMutex;
			Monitor.Enter(foxitMutex = FoxitViewer.FoxitMutex);
			RectangleF result;
			try
			{
				RectangleF rectangleF = default(RectangleF);
				double num = this.foxitLib.GetPageWidth(this.pageHandle);
				rectangleF.Width = (float)num;
				num = this.foxitLib.GetPageHeight(this.pageHandle);
				rectangleF.Height = (float)num;
				FIBR.Announce("FoxitViewer.GetPageSize", new object[]
				{
					MakeObjectID.Maker.make(this)
				});
				result = rectangleF;
			}
			finally
			{
				Monitor.Exit(foxitMutex);
			}
			return result;
		}
		public GDIBigLockedImage Render(Size outSize, Point topleft, Size pagesize, bool transparentBackground)
		{
			int alpha = transparentBackground ? 0 : 255;
			object foxitMutex;
			Monitor.Enter(foxitMutex = FoxitViewer.FoxitMutex);
			GDIBigLockedImage result;
			try
			{
				int bitmap = this.foxitLib.Bitmap_Create(outSize.Width, outSize.Height, 1);
				this.foxitLib.Bitmap_FillRect(bitmap, 0, 0, outSize.Width, outSize.Height, 255, 255, 255, alpha);
				this.foxitLib.RenderPageBitmap(bitmap, this.pageHandle, topleft.X, topleft.Y, pagesize.Width, pagesize.Height, 0, 0);
				IntPtr scan = this.foxitLib.Bitmap_GetBuffer(bitmap);
				Bitmap bitmap2 = new Bitmap(outSize.Width, outSize.Height, outSize.Width * 4, PixelFormat.Format32bppArgb, scan);
				this.foxitLib.Bitmap_Destroy(bitmap);
				GDIBigLockedImage gDIBigLockedImage = new GDIBigLockedImage(bitmap2);
				result = gDIBigLockedImage;
			}
			finally
			{
				Monitor.Exit(foxitMutex);
			}
			return result;
		}
		public RenderReply RenderBytes(Size outSize, Point topleft, Size pagesize, bool transparentBackground)
		{
			int alpha = transparentBackground ? 0 : 255;
			object foxitMutex;
			Monitor.Enter(foxitMutex = FoxitViewer.FoxitMutex);
			RenderReply result;
			try
			{
				int bitmap = this.foxitLib.Bitmap_Create(outSize.Width, outSize.Height, 1);
				this.foxitLib.Bitmap_FillRect(bitmap, 0, 0, outSize.Width, outSize.Height, 255, 255, 255, alpha);
				this.foxitLib.RenderPageBitmap(bitmap, this.pageHandle, topleft.X, topleft.Y, pagesize.Width, pagesize.Height, 0, 0);
				IntPtr source = this.foxitLib.Bitmap_GetBuffer(bitmap);
				int num = outSize.Width * 4;
				byte[] array = new byte[outSize.Height * num];
				Marshal.Copy(source, array, 0, array.Length);
				this.foxitLib.Bitmap_Destroy(bitmap);
				result = new RenderReply(array, outSize.Width * 4);
			}
			finally
			{
				Monitor.Exit(foxitMutex);
			}
			return result;
		}
		public void Dispose()
		{
			object foxitMutex;
			Monitor.Enter(foxitMutex = FoxitViewer.FoxitMutex);
			try
			{
				this.foxitLib.ClosePage(this.pageHandle);
				this.foxitLib.CloseDocument(this.docHandle);
				FIBR.Announce("FoxitViewer.Dispose", new object[]
				{
					MakeObjectID.Maker.make(this)
				});
				FoxitViewer.foxitResourceCounter.crement(-1);
			}
			finally
			{
				Monitor.Exit(foxitMutex);
			}
		}
	}
}
