using System;
namespace MSR.CVE.BackMaker
{
	public class NonredundantRenderComplaint : Exception
	{
		private string _message;
		public string message
		{
			get
			{
				return this._message;
			}
		}
		public NonredundantRenderComplaint(string message)
		{
			this._message = message;
		}
		public override int GetHashCode()
		{
			return this._message.GetHashCode();
		}
		public override bool Equals(object obj)
		{
			return obj is NonredundantRenderComplaint && this._message.Equals(((NonredundantRenderComplaint)obj)._message);
		}
		public override string ToString()
		{
			return this._message;
		}
	}
}
