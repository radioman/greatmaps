using System;
namespace MSR.CVE.BackMaker
{
	public class UnknownImageTypeException : Exception
	{
		public UnknownImageTypeException(string message) : base(message)
		{
		}
	}
}
