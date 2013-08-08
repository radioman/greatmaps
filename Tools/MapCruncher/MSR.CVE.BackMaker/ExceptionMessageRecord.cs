using System;
using System.Runtime.Serialization;
namespace MSR.CVE.BackMaker
{
	[Serializable]
	public class ExceptionMessageRecord : ISerializable
	{
		public string message;
		public ExceptionMessageRecord(string message)
		{
			this.message = message;
		}
		public ExceptionMessageRecord(SerializationInfo info, StreamingContext context)
		{
			this.message = (string)info.GetValue("Message", typeof(string));
		}
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("Message", this.message, typeof(string));
		}
		public override string ToString()
		{
			return string.Format("{0}: {1}", base.ToString(), this.message);
		}
	}
}
