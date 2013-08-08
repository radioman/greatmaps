using System;
using System.Runtime.Serialization;
namespace MSR.CVE.BackMaker
{
	[Serializable]
	public class LengthRecord : ISerializable
	{
		public int length;
		public LengthRecord(int length)
		{
			this.length = length;
		}
		public LengthRecord(SerializationInfo info, StreamingContext context)
		{
			this.length = (int)info.GetValue("Length", typeof(int));
		}
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("Length", this.length);
		}
	}
}
