using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
namespace MSR.CVE.BackMaker
{
	public class RemoteFoxitStub : IFoxitViewer, IDisposable
	{
		public const string Argument = "-remoteServer";
		private const long ChildProcessVirtualMemorySizeLimit = 1073741824L;
		private const int ChildProcessHandleCountLimit = 512;
		private string filename;
		private int pageNumber;
		private NamedPipeServer namedPipeServer;
		private RectangleF pageSize;
		public RemoteFoxitStub(string filename, int pageNumber)
		{
			this.filename = filename;
			this.pageNumber = pageNumber;
			this.Establish();
		}
		private void Establish()
		{
			if (this.namedPipeServer != null)
			{
				return;
			}
			string text = Guid.NewGuid().ToString();
			this.namedPipeServer = new NamedPipeServer(string.Format("{0} {1}", "-remoteServer", text), text);
			RectangleFRecord rectangleFRecord = (RectangleFRecord)this.namedPipeServer.RPC(new OpenRequest(this.filename, this.pageNumber));
			this.pageSize = rectangleFRecord.rect;
		}
		private void Teardown()
		{
			if (this.namedPipeServer != null)
			{
				this.namedPipeServer.Dispose();
				this.namedPipeServer = null;
			}
		}
		public void Dispose()
		{
			this.Teardown();
		}
		public RectangleF GetPageSize()
		{
			this.Establish();
			return this.pageSize;
		}
		public object RobustRPC(ISerializable request)
		{
			object result;
			try
			{
				this.Establish();
				result = this.namedPipeServer.RPC(request);
			}
			catch (Exception)
			{
				this.Teardown();
				this.Establish();
				result = this.namedPipeServer.RPC(request);
			}
			this.namedPipeServer.childProcess.Refresh();
			if (this.namedPipeServer.childProcess.VirtualMemorySize64 > 1073741824L || this.namedPipeServer.childProcess.HandleCount > 512)
			{
				this.Teardown();
			}
			return result;
		}
		public GDIBigLockedImage Render(Size outSize, Point topleft, Size pagesize, bool transparentBackground)
		{
			object obj = this.RobustRPC(new RenderRequest(topleft, pagesize, outSize, transparentBackground));
			if (obj is RenderReply)
			{
				RenderReply renderReply = (RenderReply)obj;
				if (renderReply.stride < 1 || renderReply.stride * outSize.Height > renderReply.data.Length)
				{
					throw new Exception("Invalid RenderReply");
				}
				Bitmap bitmap = new Bitmap(outSize.Width, outSize.Height, PixelFormat.Format32bppPArgb);
				BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, outSize.Width, outSize.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppPArgb);
				try
				{
					Marshal.Copy(renderReply.data, 0, bitmapData.Scan0, bitmapData.Stride * bitmapData.Height);
				}
				finally
				{
					bitmap.UnlockBits(bitmapData);
				}
				return new GDIBigLockedImage(bitmap);
			}
			else
			{
				if (obj is ExceptionMessageRecord)
				{
					throw new Exception(((ExceptionMessageRecord)obj).message);
				}
				throw new Exception(string.Format("Unrecognized reply {0}", obj.GetType()));
			}
		}
		internal long GetSize()
		{
			if (this.namedPipeServer != null && this.namedPipeServer.childProcess != null)
			{
				long workingSet = this.namedPipeServer.childProcess.WorkingSet64;
				D.Sayf(0, "Current foxit WSS {0}", new object[]
				{
					workingSet
				});
				return workingSet;
			}
			return 0L;
		}
	}
}
