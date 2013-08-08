using System;
using System.IO;
namespace MSR.CVE.BackMaker
{
	internal class StreamFilter : Stream
	{
		private Stream baseStream;
		public override bool CanRead
		{
			get
			{
				return this.baseStream.CanRead;
			}
		}
		public override bool CanSeek
		{
			get
			{
				return this.baseStream.CanSeek;
			}
		}
		public override bool CanWrite
		{
			get
			{
				return this.baseStream.CanWrite;
			}
		}
		public override long Length
		{
			get
			{
				return this.baseStream.Length;
			}
		}
		public override long Position
		{
			get
			{
				return this.baseStream.Position;
			}
			set
			{
				this.baseStream.Position = value;
			}
		}
		public StreamFilter(Stream baseStream)
		{
			this.baseStream = baseStream;
		}
		public override void Flush()
		{
			this.baseStream.Flush();
		}
		public override int Read(byte[] buffer, int offset, int count)
		{
			return this.baseStream.Read(buffer, offset, count);
		}
		public override long Seek(long offset, SeekOrigin origin)
		{
			return this.baseStream.Seek(offset, origin);
		}
		public override void SetLength(long value)
		{
			this.baseStream.SetLength(value);
		}
		public override void Write(byte[] buffer, int offset, int count)
		{
			this.baseStream.Write(buffer, offset, count);
		}
		public override void Close()
		{
			this.baseStream.Close();
		}
	}
}
