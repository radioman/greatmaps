using System;
using System.Runtime.Serialization;
namespace MSR.CVE.BackMaker
{
	[Serializable]
	public class RenderReply : ISerializable
	{
		public byte[] data;
		public int stride;
		public RenderReply(byte[] data, int stride)
		{
			this.data = data;
			this.stride = stride;
		}
		public RenderReply(byte[] sourceData, int offset, long length, int stride)
		{
			this.data = new byte[length];
			Array.Copy(sourceData, (long)offset, this.data, 0L, length);
			this.stride = stride;
		}
		public RenderReply(SerializationInfo info, StreamingContext context)
		{
			this.data = (byte[])info.GetValue("Data", typeof(byte[]));
			this.stride = (int)info.GetValue("Stride", typeof(int));
		}
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("Data", this.data);
			info.AddValue("Stride", this.stride);
		}
	}
}
