using MSR.CVE.BackMaker.ImagePipeline;
using System;
using System.Collections.Generic;
using System.Xml;
namespace MSR.CVE.BackMaker
{
	public class RegistrationDefinition : IRobustlyHashable
	{
		private List<PositionAssociation> associationList = new List<PositionAssociation>();
		private int nextPinId;
		public bool isLocked;
		private TransformationStyle _warpStyle = TransformationStyleFactory.getDefaultTransformationStyle();
		public DirtyEvent dirtyEvent;
		private static string RegistrationDefinitionTag = "RegistrationDefinition";
		public TransformationStyle warpStyle
		{
			get
			{
				return this._warpStyle;
			}
			set
			{
				if (this._warpStyle != value)
				{
					this._warpStyle = value;
					this.dirtyEvent.SetDirty();
				}
			}
		}
		public RegistrationDefinition(DirtyEvent dirtyEvent)
		{
			this.dirtyEvent = new DirtyEvent(dirtyEvent);
		}
		public RegistrationDefinition(RegistrationDefinition prototype, DirtyEvent dirtyEvent)
		{
			this.dirtyEvent = new DirtyEvent(dirtyEvent);
			if (prototype != null)
			{
				this.associationList.AddRange(prototype.associationList);
				this.isLocked = prototype.isLocked;
			}
			this.SetNextPinID();
		}
		private void SetNextPinID()
		{
			int num = -1;
			foreach (PositionAssociation current in this.associationList)
			{
				num = Math.Max(num, current.pinId);
			}
			this.nextPinId = num + 1;
		}
		public void AccumulateRobustHash(IRobustHash hash)
		{
			foreach (PositionAssociation current in this.associationList)
			{
				current.AccumulateRobustHash(hash);
			}
			this.warpStyle.AccumulateRobustHash(hash);
		}
		public override int GetHashCode()
		{
			return RobustHashTools.GetHashCode(this);
		}
		public void AddAssociation(PositionAssociation positionAssociaton)
		{
			if (positionAssociaton.pinId == -1)
			{
				positionAssociaton.pinId = this.nextPinId;
			}
			if (positionAssociaton.associationName == "")
			{
				positionAssociaton.associationName = string.Format("Pin{0}", positionAssociaton.pinId);
			}
			this.nextPinId = Math.Max(this.nextPinId, positionAssociaton.pinId) + 1;
			this.associationList.Add(positionAssociaton);
			this.dirtyEvent.SetDirty();
		}
		public void RemoveAssociation(PositionAssociation assoc)
		{
			this.associationList.Remove(assoc);
			this.dirtyEvent.SetDirty();
		}
		public List<PositionAssociation> GetAssociationList()
		{
			return this.associationList;
		}
		internal PositionAssociation GetAssocByName(string name)
		{
			foreach (PositionAssociation current in this.associationList)
			{
				if (current.associationName == name)
				{
					return current;
				}
			}
			return null;
		}
		internal bool ReadyToLock()
		{
			return this.associationList.Count >= this.warpStyle.getCorrespondencesRequired();
		}
		internal string[] GetLockStatusText()
		{
			return this.warpStyle.getDescriptionStrings(this.associationList.Count).ToArray();
		}
		public static string GetXMLTag()
		{
			return RegistrationDefinition.RegistrationDefinitionTag;
		}
		public void WriteXML(XmlTextWriter writer)
		{
			writer.WriteStartElement(RegistrationDefinition.RegistrationDefinitionTag);
			this.warpStyle.WriteXML(writer);
			foreach (PositionAssociation current in this.GetAssociationList())
			{
				current.WriteXML(writer);
			}
			writer.WriteEndElement();
		}
		public RegistrationDefinition(MashupParseContext context, DirtyEvent dirtyEvent)
		{
			this.dirtyEvent = new DirtyEvent(dirtyEvent);
			XMLTagReader xMLTagReader = context.NewTagReader(RegistrationDefinition.RegistrationDefinitionTag);
			this.warpStyle = TransformationStyleFactory.ReadFromXMLAttribute(context);
			while (xMLTagReader.FindNextStartTag())
			{
				if (xMLTagReader.TagIs(PositionAssociation.XMLTag()))
				{
					this.AddAssociation(new PositionAssociation(context, dirtyEvent));
				}
			}
		}
	}
}
