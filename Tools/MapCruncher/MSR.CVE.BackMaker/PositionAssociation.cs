using MSR.CVE.BackMaker.ImagePipeline;
using System;
using System.Globalization;
using System.Xml;
namespace MSR.CVE.BackMaker
{
	public class PositionAssociation : IRobustlyHashable
	{
		private int _pinId;
		private string _associationName;
		private string _qualityMessage;
		private DisplayablePosition _imagePosition;
		private DisplayablePosition _sourcePosition;
		private DisplayablePosition _globalPosition;
		private DirtyEvent dirtyEvent;
		private static string PositionAssociationTag = "PositionAssociation";
		private static string associationNameAttr = "associationName";
		private static string pinIdAttr = "pinId";
		private static string SourcePositionTag = "SourcePosition";
		private static string GlobalPositionTag = "GlobalPosition";
		public int pinId
		{
			get
			{
				return this._pinId;
			}
			set
			{
				this._pinId = value;
				this.dirtyEvent.SetDirty();
			}
		}
		public string associationName
		{
			get
			{
				return this._associationName;
			}
			set
			{
				this._associationName = value;
				this.dirtyEvent.SetDirty();
			}
		}
		public string qualityMessage
		{
			get
			{
				return this._qualityMessage;
			}
			set
			{
				this._qualityMessage = value;
			}
		}
		public DisplayablePosition imagePosition
		{
			get
			{
				return this._imagePosition;
			}
		}
		public DisplayablePosition sourcePosition
		{
			get
			{
				return this._sourcePosition;
			}
		}
		public DisplayablePosition globalPosition
		{
			get
			{
				return this._globalPosition;
			}
		}
		public PositionAssociation(string associationName, LatLonZoom imagePosition, LatLonZoom sourcePosition, LatLonZoom globalPosition, DirtyEvent dirtyEvent)
		{
			this.dirtyEvent = dirtyEvent;
			this._pinId = -1;
			this._associationName = associationName;
			this._imagePosition = new DisplayablePosition(imagePosition);
			this._sourcePosition = new DisplayablePosition(sourcePosition);
			this._globalPosition = new DisplayablePosition(globalPosition);
		}
		public override int GetHashCode()
		{
			return this._sourcePosition.GetHashCode() ^ this._globalPosition.GetHashCode() ^ this._pinId ^ this._associationName.GetHashCode();
		}
		public void UpdateAssociation(LatLonZoom sourceLLZ, LatLonZoom globalLLZ)
		{
			this._sourcePosition = new DisplayablePosition(sourceLLZ);
			this._globalPosition = new DisplayablePosition(globalLLZ);
			this.dirtyEvent.SetDirty();
		}
		public static string XMLTag()
		{
			return PositionAssociation.PositionAssociationTag;
		}
		public void WriteXML(XmlTextWriter writer)
		{
			writer.WriteStartElement(PositionAssociation.PositionAssociationTag);
			writer.WriteAttributeString(PositionAssociation.pinIdAttr, this._pinId.ToString(CultureInfo.InvariantCulture));
			writer.WriteAttributeString(PositionAssociation.associationNameAttr, this._associationName);
			writer.WriteStartElement(PositionAssociation.SourcePositionTag);
			this._sourcePosition.WriteXML(writer);
			writer.WriteEndElement();
			writer.WriteStartElement(PositionAssociation.GlobalPositionTag);
			this._globalPosition.WriteXML(writer);
			writer.WriteEndElement();
			writer.WriteEndElement();
		}
		public PositionAssociation(MashupParseContext context, DirtyEvent dirtyEvent)
		{
			this.dirtyEvent = dirtyEvent;
			XMLTagReader xMLTagReader = context.NewTagReader(PositionAssociation.PositionAssociationTag);
			this._pinId = -1;
			context.GetAttributeInt(PositionAssociation.pinIdAttr, ref this._pinId);
			if ((this.associationName = context.reader.GetAttribute(PositionAssociation.associationNameAttr)) == null)
			{
				this.associationName = "";
			}
			while (xMLTagReader.FindNextStartTag())
			{
				if (xMLTagReader.TagIs(PositionAssociation.SourcePositionTag))
				{
					XMLTagReader xMLTagReader2 = context.NewTagReader(PositionAssociation.SourcePositionTag);
					while (xMLTagReader2.FindNextStartTag())
					{
						if (xMLTagReader2.TagIs(DisplayablePosition.GetXMLTag(context.version)))
						{
							this._sourcePosition = new DisplayablePosition(context, ContinuousCoordinateSystem.theInstance);
							this._imagePosition = new DisplayablePosition(this._sourcePosition.pinPosition);
						}
					}
				}
				else
				{
					if (xMLTagReader.TagIs(PositionAssociation.GlobalPositionTag))
					{
						XMLTagReader xMLTagReader3 = context.NewTagReader(PositionAssociation.GlobalPositionTag);
						while (xMLTagReader3.FindNextStartTag())
						{
							if (xMLTagReader3.TagIs(DisplayablePosition.GetXMLTag(context.version)))
							{
								this._globalPosition = new DisplayablePosition(context, MercatorCoordinateSystem.theInstance);
							}
						}
					}
				}
			}
			if (this._sourcePosition == null || this._globalPosition == null)
			{
				throw new Exception(string.Format("Pin {0} does not have a source and/or global position defined", this.associationName));
			}
		}
		public void AccumulateRobustHash(IRobustHash hash)
		{
			this._sourcePosition.pinPosition.AccumulateRobustHash(hash);
			this._globalPosition.pinPosition.AccumulateRobustHash(hash);
		}
	}
}
