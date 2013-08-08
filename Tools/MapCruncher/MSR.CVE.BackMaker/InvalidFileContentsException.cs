using System;
namespace MSR.CVE.BackMaker
{
	internal class InvalidFileContentsException : Exception
	{
		public InvalidFileContentsException(string message, Exception innerException) : base(message, innerException)
		{
		}
		public InvalidFileContentsException(string message) : base(message)
		{
		}
	}
}
