using System;
namespace MSR.CVE.BackMaker
{
	internal class ConfigurationException : Exception
	{
		public ConfigurationException(string message, Exception innerException) : base(message, innerException)
		{
		}
		public ConfigurationException(string message) : base(message)
		{
		}
	}
}
