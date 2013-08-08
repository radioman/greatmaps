using System;
namespace MSR.CVE.BackMaker
{
	public class RegistrationControlRecord
	{
		public RegistrationDefinition model;
		public ReadyToLockIfc readyToLock;
		public RegistrationControlRecord(RegistrationDefinition model, ReadyToLockIfc readyToLock)
		{
			this.model = model;
			this.readyToLock = readyToLock;
		}
	}
}
