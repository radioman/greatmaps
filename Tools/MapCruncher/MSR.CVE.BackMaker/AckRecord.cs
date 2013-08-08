using System;
using System.Runtime.Serialization;
namespace MSR.CVE.BackMaker
{
	[Serializable]
	public class AckRecord : ISerializable
	{
		public AckRecord()
		{
		}
		public AckRecord(SerializationInfo info, StreamingContext context)
		{
		}
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
		}
	}
}
