using System;
namespace MSR.CVE.BackMaker
{
	public class PositionAssociationView
	{
		public enum WhichPosition
		{
			image,
			source,
			global
		}
		private PositionAssociation assoc;
		private PositionAssociationView.WhichPosition whichPosition;
		public DisplayablePosition position
		{
			get
			{
				switch (this.whichPosition)
				{
				case PositionAssociationView.WhichPosition.image:
					return this.assoc.imagePosition;
				case PositionAssociationView.WhichPosition.source:
					return this.assoc.sourcePosition;
				case PositionAssociationView.WhichPosition.global:
					return this.assoc.globalPosition;
				default:
					throw new Exception("booogus.");
				}
			}
		}
		public string associationName
		{
			get
			{
				return this.assoc.associationName;
			}
		}
		public int pinId
		{
			get
			{
				return this.assoc.pinId;
			}
		}
		public PositionAssociationView(PositionAssociation assoc, PositionAssociationView.WhichPosition whichPosition)
		{
			this.assoc = assoc;
			this.whichPosition = whichPosition;
		}
	}
}
