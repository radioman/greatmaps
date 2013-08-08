using System;
using System.Drawing;
using System.Runtime.Serialization;
namespace MSR.CVE.BackMaker
{
	[Serializable]
	public class RenderRequest : ISerializable
	{
		public Point topLeft;
		public Size pageSize;
		public Size outputSize;
		public bool transparentBackground;
		public RenderRequest(Point topLeft, Size pageSize, Size outputSize, bool transparentBackground)
		{
			this.topLeft = topLeft;
			this.pageSize = pageSize;
			this.outputSize = outputSize;
			this.transparentBackground = transparentBackground;
		}
		public RenderRequest(SerializationInfo info, StreamingContext context)
		{
			this.topLeft = (Point)info.GetValue("TopLeft", typeof(Point));
			this.pageSize = (Size)info.GetValue("PageSize", typeof(Size));
			this.outputSize = (Size)info.GetValue("OutputSize", typeof(Size));
			this.transparentBackground = (bool)info.GetValue("TransparentBackground", typeof(bool));
		}
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("TopLeft", this.topLeft);
			info.AddValue("PageSize", this.pageSize);
			info.AddValue("OutputSize", this.outputSize);
			info.AddValue("TransparentBackground", this.transparentBackground);
		}
	}
}
