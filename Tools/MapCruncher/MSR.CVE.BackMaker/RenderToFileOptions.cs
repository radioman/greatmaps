using System;
using System.Xml;
namespace MSR.CVE.BackMaker
{
	public class RenderToFileOptions : RenderToOptions
	{
		private DirtyString _outputFolder;
		public static string xmlTag = "RenderToFile";
		private static string OutputFolderAttr = "Folder";
		public string outputFolder
		{
			get
			{
				return this._outputFolder.myValue;
			}
			set
			{
				this._outputFolder.myValue = value;
			}
		}
		public RenderToFileOptions(DirtyEvent parentDirtyEvent)
		{
			this._outputFolder = new DirtyString(parentDirtyEvent);
		}
		public void WriteXML(XmlTextWriter writer)
		{
			writer.WriteStartElement(RenderToFileOptions.xmlTag);
			writer.WriteAttributeString(RenderToFileOptions.OutputFolderAttr, this.outputFolder);
			writer.WriteEndElement();
		}
		public RenderToFileOptions(MashupParseContext context, DirtyEvent parentDirtyEvent, string byTagName)
		{
			XMLTagReader xMLTagReader = context.NewTagReader(byTagName);
			this._outputFolder = new DirtyString(parentDirtyEvent);
			this.outputFolder = context.GetRequiredAttribute(RenderToFileOptions.OutputFolderAttr);
			xMLTagReader.SkipAllSubTags();
		}
		public RenderToFileOptions(MashupParseContext context, DirtyEvent parentDirtyEvent) : this(context, parentDirtyEvent, RenderToFileOptions.xmlTag)
		{
		}
		public override string ToString()
		{
			return string.Format("file:{0}", this.outputFolder);
		}
	}
}
