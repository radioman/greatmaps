using System;
namespace MSR.CVE.BackMaker
{
	public interface AssociationIfc
	{
		void AddNewAssociation(string newPinName);
		void RemoveAssociation(PositionAssociation assoc);
		void ViewAssociation(PositionAssociation pa);
		void UpdateAssociation(PositionAssociation pa, string updatedName);
		void LockMaps();
		void UnlockMaps();
	}
}
