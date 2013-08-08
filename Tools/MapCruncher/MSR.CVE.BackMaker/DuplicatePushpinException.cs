using System;
namespace MSR.CVE.BackMaker
{
	internal class DuplicatePushpinException : Exception
	{
		public string whichReference;
		public int existingPinId;
		public string existingPinName;
		public DuplicatePushpinException(string whichReference, int existingPinId, string existingPinName)
		{
			this.whichReference = whichReference;
			this.existingPinId = existingPinId;
			this.existingPinName = existingPinName;
		}
		public override string ToString()
		{
			return string.Format("The new pin has the same {0} location as pin {1} \"{2}\".", this.whichReference, this.existingPinId, this.existingPinName);
		}
	}
}
