using System;
using System.Runtime.Serialization;
namespace MSR.CVE.BackMaker
{
	[Serializable]
	public class TRecord<T> : ISerializable
	{
		public T message;
		public TRecord(T message)
		{
			this.message = message;
		}
		public TRecord(SerializationInfo info, StreamingContext context)
		{
			this.message = (T)((object)info.GetValue("Message", typeof(T)));
		}
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("Message", this.message, typeof(T));
		}
	}
}
