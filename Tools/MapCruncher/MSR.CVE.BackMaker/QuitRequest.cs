using System;
using System.Runtime.Serialization;
namespace MSR.CVE.BackMaker
{
	[Serializable]
	public class QuitRequest : ISerializable
	{
		public QuitRequest()
		{
		}
		public QuitRequest(SerializationInfo info, StreamingContext context)
		{
		}
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
		}
	}
}
