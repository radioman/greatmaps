using System;
using System.Drawing;
using System.Runtime.Serialization;
namespace MSR.CVE.BackMaker
{
	[Serializable]
	public class RectangleFRecord : ISerializable
	{
		public RectangleF rect;
		public RectangleFRecord(RectangleF rect)
		{
			this.rect = rect;
		}
		public RectangleFRecord(SerializationInfo info, StreamingContext context)
		{
			this.rect.X = (float)info.GetValue("X", typeof(float));
			this.rect.Y = (float)info.GetValue("Y", typeof(float));
			this.rect.Width = (float)info.GetValue("Width", typeof(float));
			this.rect.Height = (float)info.GetValue("Height", typeof(float));
		}
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("X", this.rect.X);
			info.AddValue("Y", this.rect.Y);
			info.AddValue("Width", this.rect.Width);
			info.AddValue("Height", this.rect.Height);
		}
	}
}
