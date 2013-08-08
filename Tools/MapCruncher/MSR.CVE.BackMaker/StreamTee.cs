using System;
using System.IO;
namespace MSR.CVE.BackMaker
{
	internal class StreamTee : Stream
	{
		private Stream inputStream;
		private Stream outputStream;
		public override bool CanRead
		{
			get
			{
				return true;
			}
		}
		public override bool CanSeek
		{
			get
			{
				return false;
			}
		}
		public override bool CanWrite
		{
			get
			{
				return false;
			}
		}
		public override long Length
		{
			get
			{
				return this.inputStream.Length;
			}
		}
		public override long Position
		{
			get
			{
				throw new Exception("The method or operation is not implemented.");
			}
			set
			{
				throw new Exception("The method or operation is not implemented.");
			}
		}
		public StreamTee(Stream inputStream, Stream outputStream)
		{
			this.inputStream = inputStream;
			this.outputStream = outputStream;
		}
		public override void Close()
		{
			this.outputStream.Close();
			base.Close();
		}
		public override void Flush()
		{
			throw new Exception("The method or operation is not implemented.");
		}
		public override int Read(byte[] buffer, int offset, int count)
		{
			int num = this.inputStream.Read(buffer, offset, count);
			if (num > 0)
			{
				this.outputStream.Write(buffer, offset, num);
			}
			return num;
		}
		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new Exception("The method or operation is not implemented.");
		}
		public override void SetLength(long value)
		{
			throw new Exception("The method or operation is not implemented.");
		}
		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}
}
